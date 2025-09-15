using dms.Models;
using dms.Dal;
using Microsoft.EntityFrameworkCore;

// host loading configurations, di container, logging
var builder = WebApplication.CreateBuilder(args);

// load documentctx into di container & configure ef core to use postgres
builder.Services.AddDbContext<DocumentContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Add services to the container
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
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
    app.UseSwaggerUI(); // takes dynamic json file and creates nice ui
}

app.UseHttpsRedirection(); // security, redirect HTTP to HTTPS
app.UseAuthorization(); // entry
app.MapControllers(); // api endpoints
app.Run(); // starts the app
