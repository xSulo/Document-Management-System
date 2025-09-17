using dms.Bl.Entities;
using dms.Bl.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace dms.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // define base url /api/documents
public class DocumentsController : ControllerBase // base because no view needed
{
    private readonly IDocumentService _svc;
    public DocumentsController(IDocumentService svc) => _svc = svc;

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
    public async Task<ActionResult<BlDocument>> Create([FromBody] BlDocument input)
    {
        var created = await _svc.AddAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] BlDocument input)
    {
        input.Id = id;
        return await _svc.UpdateAsync(input) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id) =>
        await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}
