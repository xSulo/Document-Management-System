using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AutoMapper;
using dms.Api.Configuration;
using dms.Api.Dtos;
using dms.Api.Exceptions;
using dms.Api.Messaging;
using dms.Bl.Entities;
using dms.Bl.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using dms.Api.Storage;
using dms.Api.Validation;


namespace dms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _svc;
    private readonly IMapper _mapper;
    private readonly IRabbitMqPublisher _publisher;
    private readonly IOptions<RabbitMqOptions> _mqOpt;
    private readonly ILogger<DocumentsController> _log;
    private readonly IOptions<FileStorageOptions> _storage;
    private readonly IFileStorage _fileStorage;

    public DocumentsController(IDocumentService svc, IMapper mapper, IRabbitMqPublisher publisher, IOptions<RabbitMqOptions> mqOpt, ILogger<DocumentsController> log, IOptions<FileStorageOptions> storage, IFileStorage fileStorage)
    {
        _svc = svc; _mapper = mapper; _publisher = publisher; _mqOpt = mqOpt; _log = log; _storage = storage; _fileStorage = fileStorage;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAll()
    {
        var docs = await _svc.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<DocumentDto>>(docs));
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> GetById(long id)
    {
        var doc = await _svc.GetByIdAsync(id);

        if (doc == null)
            throw new KeyNotFoundException($"Document with ID {id} not found."); 

        return Ok(_mapper.Map<DocumentDto>(doc));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<DocumentDto>> Upload([FromForm] DocumentUploadDto input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (input.File is null || input.File.Length == 0) return BadRequest("No file was uploaded.");

        var isPdfExt = Path.GetExtension(input.File.FileName)
            .Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        if (!isPdfExt) return StatusCode(StatusCodes.Status415UnsupportedMediaType, "Only .pdf files are allowed.");

        await using var tmp = input.File.OpenReadStream();

        var isPdfContent = await FileValidators.IsPdfAsync(tmp, ct); 
        if (!isPdfContent) return StatusCode(415, "Invalid PDF content.");

        if (tmp.CanSeek) tmp.Position = 0;

        var stored = await _fileStorage.SaveAsync(
            tmp,
            "application/pdf",
            Path.GetFileNameWithoutExtension(input.File.FileName),
            ct);

        var entity = new BlDocument
        {
            Title = input.Title,
            FilePath = stored.ObjectKey,
            UploadedAt = DateTime.UtcNow
        };

        var created = await _svc.AddAsync(entity);
        var dto = _mapper.Map<DocumentDto>(created);

        try
        {
            using (_log.BeginScope(new Dictionary<string, object?> { ["DocumentId"] = dto.Id, ["Title"] = dto.Title }))
            {
                _log.LogInformation("Document uploaded to MinIO, publishing OCR job...");

                var msg = new OcrJobMessage(dto.Id, dto.Title ?? "", dto.FilePath ?? "", DateTimeOffset.UtcNow);
                await _publisher.PublishAsync(_mqOpt.Value.RoutingKey, msg, ct);

                _log.LogInformation("OCR job published.");
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Publishing OCR job failed.");
            return Problem(title: "Queue unavailable", statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] DocumentUpdateDto input)
    {
        var entity = _mapper.Map<BlDocument>(input);
        entity.Id = id;

        var updated = await _svc.UpdateAsync(entity);
        if (!updated)
            throw new KeyNotFoundException($"Document with ID {id} not found.");

        var result = _mapper.Map<DocumentDto>(entity);
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _svc.DeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"Document with ID {id} not found."); // handled globally

        return NoContent();
    }
}
