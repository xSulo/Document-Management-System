using dms.Dal.Context;
using dms.Dal.Entities;
using dms.Dal.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace dms.Dal.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ILogger<DocumentRepository> _log;
    private readonly DocumentContext _ctx;

    public DocumentRepository(DocumentContext ctx, ILogger<DocumentRepository> log)
    {
        _ctx = ctx; _log = log;
    }

    public async Task<IReadOnlyList<Document>> GetAllAsync() =>
        await _ctx.Documents.AsNoTracking().ToListAsync();

    public Task<Document?> GetByIdAsync(long id) =>
        _ctx.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Document> AddAsync(Document doc)
    {
        _log.LogDebug("DB insert {Title}", doc.Title);
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