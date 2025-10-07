using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using dms.Api.Configuration;
using dms.Api.Controllers;
using dms.Api.Dtos;
using dms.Api.Messaging;
using dms.Bl.Entities;
using dms.Bl.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

public class DocumentsControllerTests
{
    private static IOptions<RabbitMqOptions> Options() =>
        Microsoft.Extensions.Options.Options.Create(new RabbitMqOptions
        {
            RoutingKey = "ocr.new"
        });

    [Fact]
    public async Task Create_Publishes_To_Rabbit_And_Returns_201()
    {
		// Arrange
		var svc = new Mock<IDocumentService>();
        var mapper = new Mock<IMapper>();
        var publisher = new Mock<IRabbitMqPublisher>();
        var log = Mock.Of<ILogger<DocumentsController>>();
        var mq = Options();

        var input = new DocumentCreateDto { Title = "Demo", FilePath = "/files/demo.pdf" };
        var blIn  = new BlDocument { Title = input.Title, FilePath = input.FilePath };
        var blOut = new BlDocument { Id = 123, Title = input.Title, FilePath = input.FilePath };
        var dtoOut = new DocumentDto { Id = 123, Title = input.Title, FilePath = input.FilePath };

        mapper.Setup(m => m.Map<BlDocument>(input)).Returns(blIn);
        svc.Setup(s => s.AddAsync(blIn)).ReturnsAsync(blOut);
        mapper.Setup(m => m.Map<DocumentDto>(blOut)).Returns(dtoOut);

		publisher
	    .Setup(p => p.PublishAsync(
		    It.IsAny<string>(),
		    It.IsAny<OcrJobMessage>(),
		    It.IsAny<CancellationToken>()))
	    .Returns(Task.CompletedTask);

        var sut = new DocumentsController(svc.Object, mapper.Object, publisher.Object, mq, log);

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await sut.Create(input);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
		var payload = Assert.IsType<DocumentDto>(created.Value);
		Assert.Equal(123, payload.Id);

		publisher.Verify(p => p.PublishAsync(
            "ocr.new",
            It.Is<OcrJobMessage>(m =>
                m.DocumentId == 123 &&
                m.Title == "Demo" &&
                m.FilePath == "/files/demo.pdf"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_When_Publisher_Fails_Returns_503()
    {
        // Arrange
        var svc = new Mock<IDocumentService>();
        var mapper = new Mock<IMapper>();
        var publisher = new Mock<IRabbitMqPublisher>();
        var log = Mock.Of<ILogger<DocumentsController>>();
        var mq = Options();

        var input = new DocumentCreateDto { Title = "Demo", FilePath = "/files/demo.pdf" };
        var bl = new BlDocument { Title = input.Title, FilePath = input.FilePath };
        var created = new BlDocument { Id = 7, Title = input.Title, FilePath = input.FilePath };
        var dto = new DocumentDto { Id = 7, Title = input.Title, FilePath = input.FilePath };

        mapper.Setup(m => m.Map<BlDocument>(input)).Returns(bl);
        svc.Setup(s => s.AddAsync(bl)).ReturnsAsync(created);
        mapper.Setup(m => m.Map<DocumentDto>(created)).Returns(dto);

        publisher
            .Setup(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<OcrJobMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("broker unreachable"));

        var sut = new DocumentsController(svc.Object, mapper.Object, publisher.Object, mq, log);

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await sut.Create(input);

        // Assert
        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(503, problem.StatusCode);
    }
}
