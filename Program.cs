using AmarTools.InvoiceGenerator.Data;
using AmarTools.InvoiceGenerator.Mappings;
using AmarTools.InvoiceGenerator.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ====================== MUST BE SET VERY EARLY ======================
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
Console.WriteLine("✅ Npgsql Legacy Timestamp Behavior ENABLED");

// ====================== Services ======================
builder.Services.AddControllersWithViews();

// PostgreSQL Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

// PDF Service
builder.Services.AddScoped<IPdfService, PdfService>();

// Response Compression
builder.Services.AddResponseCompression();

// ====================== Build App ======================
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Invoice}/{action=Generate}/{id?}");

app.Run();