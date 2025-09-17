using AutoMapper;
using dms.Api.Dtos;
using dms.Bl.Entities;
using dms.Bl.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace dms.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // define base url /api/documents
public class DocumentsController : ControllerBase // base because no view needed
{
    private readonly IDocumentService _svc;
    private readonly IMapper _mapper;
    public DocumentsController(IDocumentService svc, IMapper mapper)
    {
        _svc = svc;
        _mapper = mapper;
    }

    // ActionResult allows both return types (Ok, NotFound, etc.) and data (Document, IEnumerable<Document>)

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
        var entity = _mapper.Map<BlDocument>(input);
        var created = await _svc.AddAsync(entity);
        var dto = _mapper.Map<DocumentDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] DocumentUpdateDto input)
    {
        var entity = _mapper.Map<BlDocument>(input);
        entity.Id = id;
        var updated = await _svc.UpdateAsync(entity);
        if (!updated) return NotFound();

        var result = _mapper.Map<DocumentDto>(entity);
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id) =>
        await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}
