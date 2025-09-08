using dms.Dal;
using dms.Models;
using Microsoft.AspNetCore.Mvc;

namespace dms.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentRepository _repo;
    public DocumentsController(IDocumentRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Document>>> GetAll() =>
        Ok(await _repo.GetAllAsync());

    [HttpGet("{id:long}")]
    public async Task<ActionResult<Document>> GetById(long id)
    {
        var doc = await _repo.GetByIdAsync(id);
        return doc is null ? NotFound() : Ok(doc);
    }

    [HttpPost]
    public async Task<ActionResult<Document>> Create(Document doc)
    {
        var created = await _repo.AddAsync(doc);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, Document doc)
    {
        if (id != doc.Id) return BadRequest("ID mismatch");
        return await _repo.UpdateAsync(doc) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id) =>
        await _repo.DeleteAsync(id) ? NoContent() : NotFound();
}
