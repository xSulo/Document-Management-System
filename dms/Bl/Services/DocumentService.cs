using AutoMapper;
using FluentValidation;
using dms.Bl.Entities;
using dms.Bl.Interfaces;
using dms.Dal.Entities;
using dms.Dal.Interfaces;
using Microsoft.Extensions.Logging;

namespace dms.Bl.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repo;
    private readonly IValidator<BlDocument> _validator;
    private readonly IMapper _mapper;
    private readonly ILogger<DocumentService> _log;

    public DocumentService(
        IDocumentRepository repo,
        IValidator<BlDocument> validator,
        IMapper mapper,
        ILogger<DocumentService> log)
    {
        _repo = repo;
        _validator = validator;
        _mapper = mapper;
        _log = log;
    }

    public async Task<IReadOnlyList<BlDocument>> GetAllAsync()
    {
        _log.LogInformation("Retrieving all documents...");
        var all = await _repo.GetAllAsync();
        return _mapper.Map<IReadOnlyList<BlDocument>>(all);
    }

    public async Task<BlDocument?> GetByIdAsync(long id)
    {
        _log.LogInformation("Retrieving document with ID={Id}", id);
        var dal = await _repo.GetByIdAsync(id);
        if (dal is null)
            throw new KeyNotFoundException($"Document with ID {id} not found.");

        return _mapper.Map<BlDocument>(dal);
    }

    public async Task<BlDocument> AddAsync(BlDocument doc)
    {
        _log.LogInformation("Create document requested: Title={Title}", doc.Title);

        var res = await _validator.ValidateAsync(doc, o => o.IncludeRuleSets("Create"));
        if (!res.IsValid)
            throw new ValidationException(res.Errors);

        var dal = _mapper.Map<Document>(doc);
        var created = await _repo.AddAsync(dal);

        _log.LogInformation("Document persisted successfully: Id={Id}", created.Id);
        return _mapper.Map<BlDocument>(created);
    }

    public async Task<bool> UpdateAsync(BlDocument doc)
    {
        _log.LogInformation("Update requested: Id={Id}, Title={Title}", doc.Id, doc.Title);

        var res = await _validator.ValidateAsync(doc, o => o.IncludeRuleSets("Update"));
        if (!res.IsValid)
            throw new ValidationException(res.Errors);

        var dal = _mapper.Map<Document>(doc);
        var updated = await _repo.UpdateAsync(dal);

        if (!updated)
            throw new KeyNotFoundException($"Document with ID {doc.Id} not found for update.");

        _log.LogInformation("Document updated successfully: Id={Id}", doc.Id);
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        _log.LogInformation("Delete requested: Id={Id}", id);

        var deleted = await _repo.DeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"Document with ID {id} not found for deletion.");

        _log.LogInformation("Document deleted successfully: Id={Id}", id);
        return true;
    }
}
