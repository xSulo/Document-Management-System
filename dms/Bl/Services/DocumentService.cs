using AutoMapper;
using FluentValidation;
using dms.Bl.Entities;
using dms.Bl.Interfaces;
using dms.Dal.Entities;
using dms.Dal.Interfaces;

namespace dms.Bl.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repo;
    private readonly IValidator<BlDocument> _validator;
    private readonly IMapper _mapper;

    public DocumentService(IDocumentRepository repo, IValidator<BlDocument> validator, IMapper mapper)
    {
        _repo = repo;
        _validator = validator;
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
        var res = await _validator.ValidateAsync(doc, o => o.IncludeRuleSets("Create"));
        if (!res.IsValid) throw new ValidationException(res.Errors);

        var dal = _mapper.Map<Document>(doc);
        var created = await _repo.AddAsync(dal);
        return _mapper.Map<BlDocument>(created);
    }

    public async Task<bool> UpdateAsync(BlDocument doc)
    {
        var res = await _validator.ValidateAsync(doc, o => o.IncludeRuleSets("Update"));
        if (!res.IsValid) throw new ValidationException(res.Errors);

        var dal = _mapper.Map<Document>(doc);
        return await _repo.UpdateAsync(dal);
    }

    public Task<bool> DeleteAsync(long id) => _repo.DeleteAsync(id);
}
