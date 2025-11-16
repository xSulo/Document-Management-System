using dms.Bl.Entities;

namespace dms.Bl.Interfaces;

public interface IDocumentService
{
    Task<IReadOnlyList<BlDocument>> GetAllAsync();
    Task<BlDocument?> GetByIdAsync(long id);
    Task<BlDocument> AddAsync(BlDocument doc);
    Task<bool> UpdateAsync(BlDocument doc);
    Task<bool> DeleteAsync(long id);
    Task<bool> UpdateSummaryAsync(long id, string summary);
}
