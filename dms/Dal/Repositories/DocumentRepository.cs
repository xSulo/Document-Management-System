using System.Threading.Tasks;
using dms.Dal.Context;
using dms.Dal.Entities;
using dms.Dal.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using dms.Api.Exceptions;


namespace dms.Dal.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ILogger<DocumentRepository> _log;
    private readonly DocumentContext _ctx;

    public DocumentRepository(DocumentContext ctx, ILogger<DocumentRepository> log)
    {
        _ctx = ctx;
        _log = log;
    }

    public async Task<IReadOnlyList<Document>> GetAllAsync()
    {
        _log.LogDebug("Fetching all documents from database");
        return await _ctx.Documents.AsNoTracking().ToListAsync();
    }

    public async Task<Document?> GetByIdAsync(long id)
    {
        _log.LogDebug("Fetching document by ID={Id}", id);
        return await _ctx.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Document> AddAsync(Document doc)
    {
        try
        {
            _log.LogDebug("Inserting document {Title}", doc.Title);
            _ctx.Documents.Add(doc);
            await _ctx.SaveChangesAsync();
            _log.LogDebug("Document inserted with ID={Id}", doc.Id);
            return doc;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            _log.LogError(pgEx, "Database constraint violation while inserting document.");
            throw new DatabaseUnavailableException("Database operation failed due to constraint violation.", pgEx);
        }
        catch (PostgresException ex)
        {
            _log.LogError(ex, "Database connection failed during insert operation.");
            throw new DatabaseUnavailableException("Database connection failed.", ex);
        }
    }

    public async Task<bool> UpdateAsync(Document doc)
    {
        try
        {
            _log.LogDebug("Updating document {Id}", doc.Id);
            var exists = await _ctx.Documents.AnyAsync(d => d.Id == doc.Id);
            if (!exists) return false;

            _ctx.Documents.Update(doc);
            await _ctx.SaveChangesAsync();
            _log.LogDebug("Document updated successfully: Id={Id}", doc.Id);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _log.LogError(ex, "Concurrency issue while updating document {Id}", doc.Id);
            throw new DatabaseUnavailableException("Concurrency conflict occurred during update.", ex);
        }
        catch (PostgresException ex)
        {
            _log.LogError(ex, "Database connection failed during update operation.");
            throw new DatabaseUnavailableException("Database connection failed.", ex);
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            _log.LogDebug("Deleting document {Id}", id);
            var entity = await _ctx.Documents.FindAsync(id);
            if (entity is null)
            {
                _log.LogDebug("Document {Id} not found for deletion.", id);
                return false;
            }

            _ctx.Documents.Remove(entity);
            await _ctx.SaveChangesAsync();
            _log.LogDebug("Document {Id} deleted successfully.", id);
            return true;
        }
        catch (PostgresException ex)
        {
            _log.LogError(ex, "Database connection failed during delete operation.");
            throw new DatabaseUnavailableException("Database connection failed.", ex);
        }
    }
}
