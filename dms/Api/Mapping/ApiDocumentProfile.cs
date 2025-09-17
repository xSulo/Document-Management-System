using AutoMapper;
using dms.Bl.Entities;
using dms.Api.Dtos;

namespace dms.Api.Mapping;

public class ApiDocumentProfile : Profile
{
    public ApiDocumentProfile()
    {
        CreateMap<DocumentCreateDto, BlDocument>();
        CreateMap<DocumentUpdateDto, BlDocument>().ReverseMap();
        CreateMap<BlDocument, DocumentDto>().ReverseMap();
    }
}