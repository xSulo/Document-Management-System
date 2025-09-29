using dms.Api.Mapping;
using dms.Bl.Interfaces;
using dms.Bl.Mapping;
using dms.Bl.Services;
using dms.Dal.Context;
using dms.Dal.Interfaces;
using dms.Dal.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using dms.Validation;

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<DocumentContext>();
    ctx.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
