using TalentoPlus.Infrastructure;
using TalentoPlus.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using QuestPDF.Infrastructure;
using QuestPDF;

// QuestPDF license (Community) must be set before any document generation.
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Shared infrastructure (DbContext/Identity/Repositories) for the admin web.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    // Identity entry points must stay públicos para evitar bucles de redirección.
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/RegisterConfirmation");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ResendEmailConfirmation");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Logout");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/AccessDenied");
});
builder.Services.AddAuthorization(options =>
{
    // Force authentication everywhere unless explicitly allowed.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

await DataSeeder.SeedAsync(app.Services);

app.Run();
