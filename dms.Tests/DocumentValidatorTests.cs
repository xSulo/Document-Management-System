using System.Threading.Tasks;
using FluentValidation;
using Xunit;
using dms.Bl.Entities;
using dms.Validation;

public class DocumentValidatorTests
{
    private readonly IValidator<BlDocument> _validator = new DocumentValidator();

    private Task<FluentValidation.Results.ValidationResult> ValidateCreateAsync(BlDocument model) =>
        _validator.ValidateAsync(model, opts => opts.IncludeRuleSets("Create"));

    private Task<FluentValidation.Results.ValidationResult> ValidateUpdateAsync(BlDocument model) =>
        _validator.ValidateAsync(model, opts => opts.IncludeRuleSets("Update"));

    [Fact]
    public async Task Create_Valid_Model_Passes()
    {
        var model = new BlDocument { Title = "Spec", FilePath = "doc.pdf" };
        var result = await ValidateCreateAsync(model);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_Empty_FilePath_Fails(string path)
    {
        var model = new BlDocument { Title = "Spec", FilePath = path };
        var result = await ValidateCreateAsync(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(BlDocument.FilePath));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_Empty_Title_Fails(string? title)
    {
        var model = new BlDocument { Title = title!, FilePath = "doc.pdf" };
        var result = await ValidateCreateAsync(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(BlDocument.Title));
    }

    [Theory]
    [InlineData("file.txt")]
    [InlineData("image.gif")]
    [InlineData("data.docx")]
    public async Task Create_Unsupported_Extension_Fails(string path)
    {
        var model = new BlDocument { Title = "Spec", FilePath = path };
        var result = await ValidateCreateAsync(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Unsupported file type"));
    }

    [Fact]
    public async Task Update_Valid_Model_Passes()
    {
        var model = new BlDocument { Id = 1, Title = "Spec", FilePath = "file.jpg" };
        var result = await ValidateUpdateAsync(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Update_Id_Zero_Fails()
    {
        var model = new BlDocument { Id = 0, Title = "Spec", FilePath = "file.png" };
        var result = await ValidateUpdateAsync(model);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(BlDocument.Id));
    }
}
