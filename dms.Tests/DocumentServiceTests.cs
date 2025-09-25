using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

using dms.Bl.Entities;
using dms.Bl.Services;
using dms.Dal.Entities;
using dms.Dal.Interfaces;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _repo = new();
    private readonly Mock<IValidator<BlDocument>> _validator = new();
    private readonly Mock<IMapper> _mapper = new();

    private DocumentService CreateSut() =>
        new DocumentService(_repo.Object, _validator.Object, _mapper.Object);

    // ---------- Validator Helpers ----------
    private static ValidationResult Valid() => new();
    private static ValidationResult Invalid(string prop = "Title", string msg = "Required")
        => new(new[] { new ValidationFailure(prop, msg) });

    private void SetupValidatorOk()
    {
        // (1) T + CancellationToken
        _validator.Setup(v => v.ValidateAsync(It.IsAny<BlDocument>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Valid());

        // (2) ValidationContext<T> + CancellationToken
        _validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<BlDocument>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Valid());
    }

    private void SetupValidatorFail()
    {
        _validator.Setup(v => v.ValidateAsync(It.IsAny<BlDocument>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Invalid());

        _validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<BlDocument>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Invalid());
    }

    // ---------- Tests ----------
    [Fact]
    public async Task AddAsync_Valid_Adds_And_Maps()
    {
        SetupValidatorOk();

        var bl = new BlDocument { Title = "Spec", FilePath = "/tmp/spec.pdf" };
        var dalIn = new Document { Title = bl.Title, FilePath = bl.FilePath };
        var dalCreated = new Document { Id = 42, Title = bl.Title, FilePath = bl.FilePath };
        var blOut = new BlDocument { Id = 42, Title = bl.Title, FilePath = bl.FilePath };

        _mapper.Setup(m => m.Map<Document>(bl)).Returns(dalIn);
        _repo.Setup(r => r.AddAsync(dalIn)).ReturnsAsync(dalCreated);
        _mapper.Setup(m => m.Map<BlDocument>(dalCreated)).Returns(blOut);

        var sut = CreateSut();

        var result = await sut.AddAsync(bl);

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("Spec", result.Title);
        _repo.Verify(r => r.AddAsync(It.IsAny<Document>()), Times.Once);
        _mapper.VerifyAll();
    }

    [Fact]
    public async Task AddAsync_Invalid_Throws_ValidationException()
    {
        SetupValidatorFail();

        var bl = new BlDocument { Title = "", FilePath = "" };
        var sut = CreateSut();

        await Assert.ThrowsAsync<ValidationException>(() => sut.AddAsync(bl));

        _repo.Verify(r => r.AddAsync(It.IsAny<Document>()), Times.Never);
        _mapper.Verify(m => m.Map<Document>(It.IsAny<BlDocument>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_Maps_Back_To_BL()
    {
        var dal = new Document { Id = 7, Title = "X", FilePath = "/x" };
        var bl = new BlDocument { Id = 7, Title = "X", FilePath = "/x" };

        _repo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(dal);
        _mapper.Setup(m => m.Map<BlDocument>(dal)).Returns(bl);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(7);

        Assert.NotNull(result);
        Assert.Equal(7, result!.Id);
        _repo.Verify(r => r.GetByIdAsync(7), Times.Once);
        _mapper.Verify(m => m.Map<BlDocument>(dal), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Forwards_To_Repo()
    {
        _repo.Setup(r => r.DeleteAsync(5)).ReturnsAsync(true);

        var sut = CreateSut();
        var ok = await sut.DeleteAsync(5);

        Assert.True(ok);
        _repo.Verify(r => r.DeleteAsync(5), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Maps_And_Forwards()
    {
        SetupValidatorOk();

        var bl = new BlDocument { Id = 11, Title = "New", FilePath = "/n" };
        var dal = new Document { Id = 11, Title = "New", FilePath = "/n" };

        _mapper.Setup(m => m.Map<Document>(bl)).Returns(dal);
        _repo.Setup(r => r.UpdateAsync(dal)).ReturnsAsync(true);

        var sut = CreateSut();
        var ok = await sut.UpdateAsync(bl);

        Assert.True(ok);
        _repo.Verify(r => r.UpdateAsync(dal), Times.Once);
        _mapper.Verify(m => m.Map<Document>(bl), Times.Once);
    }
}
