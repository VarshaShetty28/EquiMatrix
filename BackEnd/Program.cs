using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BackEnd.Data;
using BackEnd.Services;

var builder = WebApplication.CreateBuilder(args);

// =============================
// CONTROLLERS + JSON SETTINGS
// =============================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Angular expects camelCase
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// =============================
// SWAGGER + JWT SUPPORT
// =============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// =============================
// SQL SERVER + EF CORE
// =============================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// =============================
// SINGLETON SERVICES
// =============================
builder.Services.AddSingleton<BackEnd.Services.ILoginService, BackEnd.Services.LoginService>();

// =============================
// TAX CONFIGURATION
// =============================
builder.Services.AddScoped<ITaxService, TaxService>();

// =============================
// MARKET PRICE SERVICE
// =============================
builder.Services.AddHttpClient<MarketPriceService>();
builder.Services.AddScoped<MarketPriceService>();

// =============================
// JWT AUTHENTICATION
// =============================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

// =============================
// CORS (Angular + Swagger)
// =============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:4200",
                    "https://localhost:4200",
                    "https://localhost:7132",
                    "http://localhost:5139"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// =============================
// BUILD APP
// =============================
var app = builder.Build();

// Swagger UI (Dev only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware order is VERY important
app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();   //  JWT Authentication
app.UseAuthorization();    //  Role-based Authorization

app.MapControllers();

app.Run();