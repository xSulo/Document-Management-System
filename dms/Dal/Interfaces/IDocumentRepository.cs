using dms.Dal.Entities;

namespace dms.Dal.Interfaces;

public interface IDocumentRepository
{
    Task<IReadOnlyList<Document>> GetAllAsync();
    Task<Document?> GetByIdAsync(long id);
    Task<Document> AddAsync(Document doc);
    Task<bool> UpdateAsync(Document doc);
    Task<bool> DeleteAsync(long id);
    Task UpdateSummaryAsync(long documentId, string summary);
}