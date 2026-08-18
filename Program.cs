using CosturaProducao.Data;
using CosturaProducao.Reports;
using CosturaProducao.Services;
using Microsoft.EntityFrameworkCore; // ESSENCIAL para UseSqlite
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// =====================================================
// BANCO DE DADOS SQLITE
// =====================================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// =====================================================
// MVC
// =====================================================

builder.Services.AddControllersWithViews();

// =====================================================
// SERVIÇOS
// =====================================================

builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<BackupService>();

var app = builder.Build();

// =====================================================
// TRATAMENTO DE ERROS
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// =====================================================
// ARQUIVOS ESTÁTICOS
// =====================================================

app.UseStaticFiles();

// =====================================================
// ROTEAMENTO
// =====================================================

app.UseRouting();

// =====================================================
// ROTA PRINCIPAL
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// =====================================================
// BANCO / SEED
// =====================================================

await SeedData.InitializeAsync(
    app.Services,
    app.Configuration
);

// =====================================================
// INICIA APLICAÇÃO
// =====================================================

await app.RunAsync();

public partial class Program { }