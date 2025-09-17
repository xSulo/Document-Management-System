using AutoMapper;
using dms.Bl.Entities;
using dms.Bl.Interfaces;
using dms.Dal.Entities;
using dms.Dal.Interfaces;

namespace dms.Bl.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repo;
    private readonly IMapper _mapper;     
    public DocumentService(IDocumentRepository repo, IMapper mapper)
    {
        _repo = repo; 
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<BlDocument>> GetAllAsync() =>
        _mapper.Map<IReadOnlyList<BlDocument>>(await _repo.GetAllAsync());

    public async Task<BlDocument?> GetByIdAsync(long id)
    {
        var dal = await _repo.GetByIdAsync(id);
        return dal is null ? null : _mapper.Map<BlDocument>(dal);
    }

    public async Task<BlDocument> AddAsync(BlDocument doc)
    {
        if (string.IsNullOrWhiteSpace(doc.Title))
            throw new ArgumentException("Title is required.", nameof(doc.Title));

        var dal = _mapper.Map<Document>(doc);
        var created = await _repo.AddAsync(dal);
        return _mapper.Map<BlDocument>(created);
    }

    public async Task<bool> UpdateAsync(BlDocument doc)
    {
        var dal = _mapper.Map<Document>(doc);
        return await _repo.UpdateAsync(dal);
    }

    public Task<bool> DeleteAsync(long id) => _repo.DeleteAsync(id);
}
