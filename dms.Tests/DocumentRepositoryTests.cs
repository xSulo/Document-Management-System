using System;
using System.Threading.Tasks;
using dms.Dal.Context;
using dms.Dal.Entities;
using dms.Dal.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class DocumentRepositoryTests
{
    private static (DocumentRepository Repo, DocumentContext Ctx, SqliteConnection Conn) CreateRepo()
    {
        // relational In-Memory DB (SQLite) -> more realistic tests than EF InMemory
        var conn = new SqliteConnection("Filename=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<DocumentContext>()
            .UseSqlite(conn)
            .Options;

        var ctx = new DocumentContext(options);
        ctx.Database.EnsureCreated();   // EnsureCreated = create tables in memory
        return (new DocumentRepository(ctx), ctx, conn);
    }

    [Fact]
    public async Task Add_And_Get_By_Id_Works()
    {
        var (repo, ctx, conn) = CreateRepo();
        try
        {
            var created = await repo.AddAsync(new Document { Title = "Spec", FilePath = "/tmp/spec.pdf" });
            var loaded = await repo.GetByIdAsync(created.Id);

            Assert.NotNull(loaded);
            Assert.Equal("Spec", loaded!.Title);
            Assert.Equal("/tmp/spec.pdf", loaded.FilePath);
            Assert.True(created.Id > 0);
        }
        finally { ctx.Dispose(); conn.Dispose(); }
    }

    [Fact]
    public async Task Update_NotFound_ReturnsFalse()
    {
        var (repo, ctx, conn) = CreateRepo();
        try
        {
            var ok = await repo.UpdateAsync(new Document { Id = 999, Title = "X", FilePath = "/x" });
            Assert.False(ok);
        }
        finally { ctx.Dispose(); conn.Dispose(); }
    }

    [Fact]
    public async Task Delete_Works()
    {
        var (repo, ctx, conn) = CreateRepo();
        try
        {
            var d = await repo.AddAsync(new Document { Title = "Del", FilePath = "/del.pdf" });
            var ok = await repo.DeleteAsync(d.Id);

            Assert.True(ok);
            Assert.Null(await repo.GetByIdAsync(d.Id));
        }
        finally { ctx.Dispose(); conn.Dispose(); }
    }

    [Fact]
    public async Task GetAll_Returns_All_Items()
    {
        var (repo, ctx, conn) = CreateRepo();
        try
        {
            await repo.AddAsync(new Document { Title = "A", FilePath = "/a" });
            await repo.AddAsync(new Document { Title = "B", FilePath = "/b" });

            var all = await repo.GetAllAsync();
            Assert.True(all.Count >= 2);
            Assert.Contains(all, d => d.Title == "A");
            Assert.Contains(all, d => d.Title == "B");
        }
        finally { ctx.Dispose(); conn.Dispose(); }
    }

    [Fact]
    public async Task Update_Works()
    {
        var (repo, ctx, conn) = CreateRepo();
        try
        {
            var d = await repo.AddAsync(new Document { Title = "Old", FilePath = "/x" });
            d.Title = "New";

            var ok = await repo.UpdateAsync(d);
            Assert.True(ok);

            var reloaded = await repo.GetByIdAsync(d.Id);
            Assert.Equal("New", reloaded!.Title);
        }
        finally { ctx.Dispose(); conn.Dispose(); }
    }

    [Fact]
    public async Task Delete_NotFound_ReturnsFalse()
    {
        var (repo, ctx, conn) = CreateRepo();
        try
        {
            var ok = await repo.DeleteAsync(123456);
            Assert.False(ok);
        }
        finally { ctx.Dispose(); conn.Dispose(); }
    }
}
