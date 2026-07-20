using SportUp.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// Add Swagger (Swashbuckle) for a user-friendly UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure EF Core DbContext (PostgreSQL for development)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=SportUpDb;Username=postgres;Password=YourStrong!Password";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
// Ensure static files are served (OpenAPI UI uses embedded static assets)
app.UseStaticFiles();
// Serve Swagger UI (Swashbuckle) and point it at the generated Swagger JSON
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Point Swagger UI to the Swashbuckle endpoint
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SportUp API v1");
    // Serve the UI under /openapi/ui
    c.RoutePrefix = "openapi/ui";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
