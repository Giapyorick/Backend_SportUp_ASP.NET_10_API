using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SportUp.Data;
using SportUp.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== ÉP ASP.NET CORE NẠP CHÍNH XÁC ASSEMBLY CHỨA AUTHCONTROLLER =====
builder.Services.AddControllers()
    .AddApplicationPart(typeof(SportUp.Controllers.AuthController).Assembly);

// ===== 1. CẤU HÌNH COOKIE POLICY CHO CROSS-ORIGIN (VERCEL -> RUNASP) =====
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.OnAppendCookie = cookieContext =>
    {
        cookieContext.CookieOptions.SameSite = SameSiteMode.None;
        cookieContext.CookieOptions.Secure = true;
    };
});

// ===== 2. READ JWT SETTINGS & CONFIGURE AUTHENTICATION =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

// Đọc chính xác Key, Issuer, Audience từ appsettings.json
var secretStr = jwtSettings["SecretKey"] ?? "MLLDSi8wLAva8b+NEXa0G+PnOeG790dmUoXHtnwyOGg=";
var secretKey = Encoding.UTF8.GetBytes(secretStr);

var issuerStr = jwtSettings["Issuer"] ?? "SportUpApi";
var audienceStr = jwtSettings["Audience"] ?? "SportUpReactAdmin";

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
        ValidIssuer = issuerStr,          // "SportUpApi"
        ValidAudience = audienceStr,      // "SportUpReactAdmin"
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.FromMinutes(5) // Chống lệch giờ máy chủ
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<TokenService>();

// ===== 3. CORS =====
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

// ===== 4. OPENAPI / SWAGGER =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== 5. CONFIGURE EF CORE DBCONTEXT FOR SQL SERVER =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

app.UseCookiePolicy();
app.UseStaticFiles();

// ===== 6. SCALAR UI & SWAGGER JSON =====
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

// ===== THỨ TỰ BẮT BUỘC: CORS -> AUTHENTICATION -> AUTHORIZATION =====
app.UseCors("ReactPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();