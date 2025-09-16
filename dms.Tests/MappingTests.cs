using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using dms.Bl.Mapping;
using dms.Api.Mapping;

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

        // Act improve test later!!!!!!! CHECK FOR CORRECT DATA
        var mapper = provider.GetRequiredService<IMapper>();

        // Assert
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
