using CosturaProducao.Data;
using CosturaProducao.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosturaProducao.Controllers;



public sealed class PdfController(ApplicationDbContext db, IPdfService pdf, IWebHostEnvironment environment) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Assignment(int id)
    {
        var item = await db.Assignments.AsNoTracking().Include(x => x.Seamstress).Include(x => x.ProductionProcess).ThenInclude(x => x.ServiceProcess).Include(x => x.ProductionProcess).ThenInclude(x => x.Production).ThenInclude(x => x.Client).Include(x => x.ProductionProcess).ThenInclude(x => x.Production).ThenInclude(x => x.PieceModel).SingleOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        var production = item.ProductionProcess.Production;
        var model = new FichaPdfModel(item.Seamstress.Name, production.Client.Name, production.PieceModel.Name, production.Color, item.PlannedQuantity, item.ProductionProcess.ServiceProcess.Name, item.PricePerPiece, item.TotalAmount, production.DeliveryDate, production.Notes, ResolveTemplate(production.PieceModel.TemplateImagePath));
        return File(pdf.Assignment(model), "application/pdf", $"ficha-{id}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> Template(int id)
    {
        var piece = await db.PieceModels.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (piece is null) return NotFound();
        var model = new TemplatePdfModel(piece.Name, piece.Code, piece.Color, piece.Description, ResolveTemplate(piece.TemplateImagePath));
        return File(pdf.Template(model), "application/pdf", $"gabarito-{piece.Code}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> Monthly(int? year, int? month)
    {
        var today = DateTime.Today;
        var selectedYear = year is >= 2000 and <= 2100 ? year.Value : today.Year;
        var selectedMonth = month is >= 1 and <= 12 ? month.Value : today.Month;
        var start = new DateTime(selectedYear, selectedMonth, 1);
        var end = start.AddMonths(1);
        var assignments = await db.Assignments.AsNoTracking().Include(x => x.Seamstress).Include(x => x.ProductionProcess).ThenInclude(x => x.Production).ThenInclude(x => x.Client).Where(x => x.ProductionProcess.Production.ProductionDate >= start && x.ProductionProcess.Production.ProductionDate < end).ToListAsync();
        var values = assignments.Select(x => new { Name = x.Seamstress.Name, Client = x.ProductionProcess.Production.Client.Name, Quantity = x.ProducedQuantity, Amount = x.ProducedQuantity * x.PricePerPiece }).ToList();
        var model = new MonthlyPdfModel(selectedYear, selectedMonth, values.Sum(x => x.Quantity), values.Sum(x => x.Amount), values.GroupBy(x => x.Name).Select(g => new MonthlyPdfRow(g.Key, g.Sum(x => x.Quantity), g.Sum(x => x.Amount))).OrderByDescending(x => x.Amount).ToList(), values.GroupBy(x => x.Client).Select(g => new MonthlyPdfRow(g.Key, g.Sum(x => x.Quantity), g.Sum(x => x.Amount))).OrderByDescending(x => x.Amount).ToList());
        return File(pdf.Monthly(model), "application/pdf", $"relatorio-{selectedYear}-{selectedMonth:00}.pdf");
    }

    private string? ResolveTemplate(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return null;
        var root = Path.GetFullPath(environment.WebRootPath);
        var full = Path.GetFullPath(Path.Combine(root, relativePath.TrimStart('/', '\\')));
        return full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(full) ? full : null;
    }
}