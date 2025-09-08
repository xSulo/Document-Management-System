using dms.Dal;
using dms.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class DocumentRepositoryTests
{
    private static DocumentRepository CreateRepo()
    {
        var opts = new DbContextOptionsBuilder<DocumentContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DocumentRepository(new DocumentContext(opts));
    }

    [Fact]
    public async Task Add_And_Get_By_Id_Works()
    {
        var repo = CreateRepo();
        var created = await repo.AddAsync(new Document { Title = "Spec", FilePath = "/tmp/spec.pdf" });

        var loaded = await repo.GetByIdAsync(created.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Spec", loaded!.Title);
    }
}
