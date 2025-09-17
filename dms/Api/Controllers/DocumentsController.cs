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
    public async Task<ActionResult<IEnumerable<BlDocument>>> GetAll() =>
        Ok(await _svc.GetAllAsync());

    [HttpGet("{id:long}")]
    public async Task<ActionResult<BlDocument>> GetById(long id)
    {
        var doc = await _svc.GetByIdAsync(id);
        return doc is null ? NotFound() : Ok(doc);
    }

    [HttpPost]
    public async Task<ActionResult<BlDocument>> Create([FromBody] DocumentCreateDto input)
    {
        var entity = _mapper.Map<BlDocument>(input);
        var created = await _svc.AddAsync(entity);
        var dto = _mapper.Map<DocumentDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] DocumentCreateDto input)
    {
        var entity = _mapper.Map<BlDocument>(input);
        entity.Id = id;
        var updated = await _svc.UpdateAsync(entity);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id) =>
        await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}
