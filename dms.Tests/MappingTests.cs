using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using dms.Bl.Mapping;
using dms.Api.Mapping;
using dms.Api.Dtos;
using dms.Bl.Entities;

public class MappingTests
{
	[Fact]
	public void MapperConfiguration_IsValid()
	{
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<DocumentProfile>();
            cfg.AddProfile<ApiDocumentProfile>();
        });

        var provider = services.BuildServiceProvider();

        // Act
        var mapper = provider.GetRequiredService<IMapper>();

        // Assert
        var createDto = new DocumentCreateDto { Title = "Spec", FilePath = "file.pdf" };
        var blFromCreate = mapper.Map<BlDocument>(createDto);
        Assert.Equal("Spec", blFromCreate.Title);
        Assert.Equal("file.pdf", blFromCreate.FilePath);

        var updateDto = new DocumentUpdateDto { Title = "New", FilePath = "img.jpg" };
        var blFromUpdate = mapper.Map<BlDocument>(updateDto);
        Assert.Equal("New", blFromUpdate.Title);
        Assert.Equal("img.jpg", blFromUpdate.FilePath);

        var bl = new BlDocument { Id = 7, Title = "X", FilePath = "/x" };
        var apiDto = mapper.Map<DocumentDto>(bl);
        Assert.Equal(7, apiDto.Id);
        Assert.Equal("X", apiDto.Title);
        Assert.Equal("/x", apiDto.FilePath);
    }
}
