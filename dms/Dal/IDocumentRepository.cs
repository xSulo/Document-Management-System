using dms.Models;

namespace dms.Dal;

public interface IDocumentRepository
{
    Task<IReadOnlyList<Document>> GetAllAsync();
    Task<Document?> GetByIdAsync(long id);
    Task<Document> AddAsync(Document doc);
    Task<bool> UpdateAsync(Document doc);
    Task<bool> DeleteAsync(long id);
}