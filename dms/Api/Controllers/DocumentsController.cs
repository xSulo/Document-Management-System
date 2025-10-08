using AutoMapper;
using FluentValidation;
using dms.Api.Dtos;
using dms.Bl.Entities;
using dms.Bl.Interfaces;
using Microsoft.AspNetCore.Mvc;
using dms.Api.Messaging;
using dms.Api.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using dms.Api.Exceptions;


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

    public DocumentsController(IDocumentService svc, IMapper mapper, IRabbitMqPublisher publisher, IOptions<RabbitMqOptions> mqOpt, ILogger<DocumentsController> log, IOptions<FileStorageOptions> storage)
    {
        _svc = svc; _mapper = mapper; _publisher = publisher; _mqOpt = mqOpt; _log = log; _storage = storage;
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
    public async Task<ActionResult<DocumentDto>> Upload([FromForm] DocumentUploadDto input)
    {
        if (input.File == null || input.File.Length == 0)
            return BadRequest("No file was uploaded.");

        if (!input.File.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only PDF files are allowed.");

        var root = _storage.Value.UploadsRoot;
        Directory.CreateDirectory(root);

        var originalBase = Path.GetFileNameWithoutExtension(input.File.FileName);
        var safeBase = string.Concat(originalBase.Where(char.IsLetterOrDigit));
        if (string.IsNullOrWhiteSpace(safeBase)) safeBase = "document";

        var uniqueName = $"{safeBase}_{Guid.NewGuid():N}.pdf";
        var absPath = Path.Combine(root, uniqueName);

        await using (var fs = System.IO.File.Create(absPath))
        {
            await input.File.CopyToAsync(fs);
        }

        var entity = new BlDocument
        {
            Title = input.Title,
            FilePath = uniqueName,
            UploadedAt = DateTime.UtcNow
        };

        
        var created = await _svc.AddAsync(entity);
        var dto = _mapper.Map<DocumentDto>(created);

        using (_log.BeginScope(new Dictionary<string, object?>
        {
            ["DocumentId"] = dto.Id,
            ["Title"] = dto.Title
        }))
        {
            _log.LogInformation("Document uploaded, publishing OCR job...");


            var absoluteForWorker = Path.Combine(root, dto.FilePath ?? string.Empty);
            var message = new OcrJobMessage(
                dto.Id,
                dto.Title ?? string.Empty,
                absoluteForWorker,
                DateTimeOffset.UtcNow
            );

            try
            {
                await _publisher.PublishAsync(_mqOpt.Value.RoutingKey, message);
                _log.LogInformation("OCR job published successfully.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Publishing OCR job failed.");
                throw new QueueUnavailableException("Failed to publish OCR job to RabbitMQ.", ex);
            }
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
