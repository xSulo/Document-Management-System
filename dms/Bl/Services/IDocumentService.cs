using dms.Bl.Entities;

public interface IDocumentService
{
    Task<IReadOnlyList<BlDocument>> GetAllAsync();
    Task<BlDocument?> GetByIdAsync(long id);
    Task<BlDocument> AddAsync(BlDocument doc);
    Task<bool> UpdateAsync(BlDocument doc);
    Task<bool> DeleteAsync(long id);
}
