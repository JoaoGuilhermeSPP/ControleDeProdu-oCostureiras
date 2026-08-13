using CosturaProducao.Data;
using CosturaProducao.Reports;
using CosturaProducao.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Banco de dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "A configuração ConnectionStrings:DefaultConnection não foi encontrada."
    );

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

// MVC
builder.Services.AddControllersWithViews();

// Serviços
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<BackupService>();

var app = builder.Build();

// Tratamento de erros
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Arquivos estáticos
app.UseStaticFiles();

// Roteamento
app.UseRouting();

// Rota principal
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Inicialização dos dados
await SeedData.InitializeAsync(
    app.Services,
    app.Configuration
);

// Inicia aplicação
await app.RunAsync();

public partial class Program { }