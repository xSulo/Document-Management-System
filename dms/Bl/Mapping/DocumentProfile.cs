using AutoMapper;
using dms.Bl.Entities;
using dms.Dal.Entities;

namespace dms.Bl.Mapping;

public class DocumentProfile : Profile
{
    public DocumentProfile()
    {
        CreateMap<BlDocument, Document>()
            .ForMember(dest => dest.UploadedAt, opt => opt.Ignore());
        CreateMap<Document, BlDocument>();
    }
}