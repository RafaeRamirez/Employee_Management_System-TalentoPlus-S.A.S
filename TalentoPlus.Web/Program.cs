using TalentoPlus.Infrastructure;
using TalentoPlus.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Shared infrastructure (DbContext/Identity/Repositories) for the admin web.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
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
