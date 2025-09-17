using dms.Dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace dms.Dal.Context;

public class DocumentContext : DbContext
{
    public DocumentContext(DbContextOptions<DocumentContext> options) : base(options) { }
    public DbSet<Document> Documents => Set<Document>();
}