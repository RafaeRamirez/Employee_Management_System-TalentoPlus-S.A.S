using TalentoPlus.Infrastructure;
using TalentoPlus.Infrastructure.Data;
using QuestPDF.Infrastructure;
using QuestPDF;

// QuestPDF license (Community) must be set before any document generation.
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration, useJwtAsDefault: true);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger is enabled for both dev and prod to ease testing; secure it externally if needed.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await DataSeeder.SeedAsync(app.Services);

app.Run();
