using dms.Dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace dms.Dal.Context;

public class DocumentContext : DbContext
{
    // ctor receives configuration options (eg connection string) from di container
    public DocumentContext(DbContextOptions<DocumentContext> options) : base(options) { }

    // Represents the 'Documents' table in the database
    // Enables CRUD operations on Document entities without writing raw SQL
    // EF Core translates operations on this DbSet into SQL commands
    public DbSet<Document> Documents => Set<Document>();
}