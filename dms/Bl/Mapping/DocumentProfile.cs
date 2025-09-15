using AutoMapper;
using dms.Bl.Entities;
using dms.Dal.Entities;

namespace dms.Bl.Mapping;

public class DocumentProfile : Profile
{
    public DocumentProfile()
    {
        CreateMap<Document, BlDocument>().ReverseMap();
    }
}