using dms.Dal;
using dms.Dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace dms.Dal;

public class DocumentRepository : IDocumentRepository
{
    private readonly DocumentContext _ctx;
    public DocumentRepository(DocumentContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Document>> GetAllAsync() =>
        await _ctx.Documents.AsNoTracking().ToListAsync();

    public Task<Document?> GetByIdAsync(long id) =>
        _ctx.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Document> AddAsync(Document doc)
    {
        _ctx.Documents.Add(doc);
        await _ctx.SaveChangesAsync();
        return doc;
    }

    public async Task<bool> UpdateAsync(Document doc)
    {
        var exists = await _ctx.Documents.AnyAsync(d => d.Id == doc.Id);
        if (!exists) return false;
        _ctx.Documents.Update(doc);
        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _ctx.Documents.FindAsync(id);
        if (entity is null) return false;
        _ctx.Documents.Remove(entity);
        await _ctx.SaveChangesAsync();
        return true;
    }
}