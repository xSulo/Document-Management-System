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

    public DocumentsController(IDocumentService svc, IMapper mapper, IRabbitMqPublisher publisher, IOptions<RabbitMqOptions> mqOpt, ILogger<DocumentsController> log)
    {
        _svc = svc; _mapper = mapper; _publisher = publisher; _mqOpt = mqOpt; _log = log;
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
        return doc is null ? NotFound() : Ok(_mapper.Map<DocumentDto>(doc));
    }

    [HttpPost]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentDto>> Create([FromBody] DocumentCreateDto input)
    {
        try
        {
            var entity = _mapper.Map<BlDocument>(input);
            var created = await _svc.AddAsync(entity);
            var dto = _mapper.Map<DocumentDto>(created);

            using (_log.BeginScope(new Dictionary<string, object>
            {
                ["DocumentId"] = dto.Id,
                ["Title"] = dto.Title ?? string.Empty
            }))
            {
                _log.LogInformation("Document created, publishing OCR job...");
                try
                {
                    await _publisher.PublishAsync(
                        _mqOpt.Value.RoutingKey,
                        new OcrJobMessage(dto.Id, dto.Title ?? string.Empty, dto.FilePath ?? string.Empty, DateTimeOffset.UtcNow),
                        HttpContext.RequestAborted);

                    _log.LogInformation("OCR job published.");
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Publishing OCR job failed.");
                    return Problem(title: "Queue unavailable", statusCode: 503);
                }
            }

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return ValidationProblem(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed"
            });
        }
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] DocumentUpdateDto input)
    {
        try
        {
            var entity = _mapper.Map<BlDocument>(input);
            entity.Id = id;

            var updated = await _svc.UpdateAsync(entity);
            if (!updated) return NotFound();

            var result = _mapper.Map<DocumentDto>(entity);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return ValidationProblem(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed"
            });
        }
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id) =>
        await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}
