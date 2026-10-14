using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using SportUp.Data;
using SportUp.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== ÉP ASP.NET CORE NẠP CHÍNH XÁC ASSEMBLY CHỨA AUTHCONTROLLER =====
builder.Services.AddControllers()
    .AddApplicationPart(typeof(SportUp.Controllers.AuthController).Assembly);

// ===== 1. Read JWT Settings & Configure Authentication =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

// Bọc fallback an toàn nếu SecretKey bị rỗng hoặc chưa inject kịp
var secretStr = jwtSettings["SecretKey"] ?? "SportUp_Super_Secret_Key_For_Jwt_Authentication_2026_Minimum_32_Bytes!";
var secretKey = Encoding.UTF8.GetBytes(secretStr);

var issuerStr = jwtSettings["Issuer"] ?? "SportUpIssuer";
var audienceStr = jwtSettings["Audience"] ?? "SportUpAudience";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuerStr,
        ValidAudience = audienceStr,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<TokenService>();

// ===== 2. CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://fontend-sport-up-react.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ===== 3. OpenAPI / Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SportUp API", Version = "v1" });
});

// ===== 4. Configure EF Core DbContext for SQL Server =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

app.UseStaticFiles();

// ===== 5. SCALAR UI & SWAGGER JSON =====
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.MapScalarApiReference(options =>
{
    options.WithTitle("SportUp API Reference")
           .WithTheme(ScalarTheme.Purple)
           .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
           .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch);
});

app.UseHttpsRedirection();

// ===== Enable CORS & Auth =====
app.UseCors("ReactPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();  