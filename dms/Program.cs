using dms.Api.Mapping;
using dms.Bl.Interfaces;
using dms.Bl.Mapping;
using dms.Bl.Services;
using dms.Dal.Context;
using dms.Dal.Interfaces;
using dms.Dal.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DocumentContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Add services to the container
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

// Add AutoMapper and register mapping profiles
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<DocumentProfile>();
    cfg.AddProfile<ApiDocumentProfile>();
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build the application host with all registered services and configuration
var app = builder.Build();

using (var scope = app.Services.CreateScope()) // create scope to get dbcontext to migrate
{
    var ctx = scope.ServiceProvider.GetRequiredService<DocumentContext>();
    ctx.Database.Migrate();   // creates/updates tables
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // create static json file /openapi/v1.json
    app.UseSwagger(); // http://localhost:5032/swagger/v1/swagger.json creates dynamic json file
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
