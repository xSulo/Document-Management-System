using dms.Api.Mapping;
using dms.Api.Configuration;
using dms.Api.Messaging;
using dms.Bl.Interfaces;
using dms.Bl.Mapping;
using dms.Bl.Services;
using dms.Dal.Context;
using dms.Dal.Interfaces;
using dms.Dal.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using FluentValidation;
using dms.Validation;
using dms.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DocumentContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<DocumentProfile>();
    cfg.AddProfile<ApiDocumentProfile>();
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<DocumentValidator>();
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<DocumentContext>();
    for (int i = 0; i < 10; i++)
    {
        try
        {
            ctx.Database.Migrate();
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DB not ready ({ex.Message}), retrying in 3s...");
            Thread.Sleep(3000);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseGlobalExceptionHandling();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "storage", "uploads");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/files"
});

app.MapGet("/health", async (dms.Dal.Context.DocumentContext db) =>
{
    try { return await db.Database.CanConnectAsync() ? Results.Ok(new { status = "Healthy" }) : Results.StatusCode(503); }
    catch { return Results.StatusCode(503); }
});

app.Run();
