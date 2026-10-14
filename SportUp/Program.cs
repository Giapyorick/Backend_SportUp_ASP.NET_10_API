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

// ===== 1. CẤU HÌNH COOKIE POLICY CHO CROSS-ORIGIN (VERCEL -> RUNASP) =====
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.OnAppendCookie = cookieContext =>
    {
        cookieContext.CookieOptions.SameSite = SameSiteMode.None;
        cookieContext.CookieOptions.Secure = true; // Bắt buộc khi dùng SameSite=None
    };
});

// ===== 2. READ JWT SETTINGS & CONFIGURE AUTHENTICATION =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

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
        ClockSkew = TimeSpan.FromMinutes(5) // 👈 Chống lệch múi giờ giữa Server & Client
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
            .AllowCredentials(); // Bắt buộc gửi/nhận RefreshToken Cookie
    });
});

// ===== 4. OPENAPI / SWAGGER (TÍCH HỢP BEARER AUTHORIZE) =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SportUp API", Version = "v1" });

    // Khai báo cấu hình JWT Bearer cho Scalar/Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập AccessToken vào đây (ví dụ: eyJhbGci...)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===== 5. CONFIGURE EF CORE DBCONTEXT FOR SQL SERVER =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

app.UseCookiePolicy(); // 👈 Bật Cookie Policy
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

// ===== ENABLE CORS & AUTH =====
app.UseCors("ReactPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();