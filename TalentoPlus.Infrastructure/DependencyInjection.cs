using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TalentoPlus.Application.Interfaces;
using TalentoPlus.Infrastructure.Data;
using TalentoPlus.Infrastructure.Identity;
using TalentoPlus.Infrastructure.Services;
using TalentoPlus.Infrastructure.Repositories;

namespace TalentoPlus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool useJwtAsDefault = false)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

        services.ConfigureApplicationCookie(options =>
        {
            // Force the login endpoints to the built-in Identity UI.
            options.LoginPath = "/Identity/Account/Login";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        });

        // Web uses cookie auth by default; API can force JWT as the primary scheme.
        var authBuilder = useJwtAsDefault
            ? services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            : services.AddAuthentication();

        authBuilder.AddJwtBearer(options =>
        {
            var secret = configuration["Jwt__Secret"] ?? "very_secret_key_123456789";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt__Issuer"] ?? "TalentoPlus.Api",
                ValidAudience = configuration["Jwt__Audience"] ?? "TalentoPlus.Client",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
            };
        });

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddHttpClient<IAiProvider, AiProvider>();

        return services;
    }
}
