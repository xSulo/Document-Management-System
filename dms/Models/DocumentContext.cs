using Microsoft.EntityFrameworkCore;

namespace dms.Models;

public class DocumentContext : DbContext
{
    public DocumentContext(DbContextOptions<DocumentContext> options) : base(options) { }
    public DbSet<Document> Documents => Set<Document>();
}