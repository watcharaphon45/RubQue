using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RubQue.Application;
using RubQue.Application.Auth.Interfaces;
using RubQue.Application.Common.Interfaces;
using RubQue.Infrastructure;
using RubQue.Infrastructure.Persistence;
using RubQue.WebApi.Middlewares;
using RubQue.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Clean Architecture Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. HTTP Context & Current User
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 3. Controllers
builder.Services.AddControllers();

// 4. Authentication (JWT Bearer)
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "RubQueSuperSecretKeyLongEnoughForHmacSha256Security2026!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RubQueApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "RubQueClients";

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
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 5. CORS for LIFF and Web Admin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 6. Swagger with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RubQue API - Barber & Hair Salon Booking Platform",
        Version = "v1",
        Description = "Core RESTful API for RubQue Booking Platform (Clean Architecture + ASP.NET Core)"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Format: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

var app = builder.Build();

// 7. Middlewares
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RubQue API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at application root (http://localhost:5000)
    });
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 8. Auto-apply migrations and seed data on startup (if database is reachable)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<RubQueDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        if (!context.Database.IsRelational())
        {
            logger.LogInformation("Using In-Memory Database with Mockup Data.");
            await context.Database.EnsureCreatedAsync();
            await DbInitializer.SeedAsync(context, passwordHasher);
            logger.LogInformation("In-Memory Database initialized and mock data seeded successfully.");
        }
        else
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            logger.LogInformation("Checking database connectivity (timeout 3s)...");
            var canConnect = await context.Database.CanConnectAsync(cts.Token);

            if (canConnect)
            {
                logger.LogInformation("Database connected. Applying migrations/ensuring schema...");
                await context.Database.EnsureCreatedAsync();
                await DbInitializer.SeedAsync(context, passwordHasher);
                logger.LogInformation("Database initialized and demo data seeded successfully.");
            }
            else
            {
                logger.LogWarning("PostgreSQL is not reachable at the moment. Starting API without initial DB seeding.");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning("Could not connect to database on startup: {Message}. (Start PostgreSQL via Docker or local service)", ex.Message);
    }
}

app.Run();
