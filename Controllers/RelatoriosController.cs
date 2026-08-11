using CosturaProducao.Data;
using CosturaProducao.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosturaProducao.Controllers;

[Authorize]
public sealed class RelatoriosController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? seamstressId)
    {
        var query = db.Assignments.AsNoTracking().Include(x => x.Seamstress).Include(x => x.ProductionProcess).ThenInclude(x => x.Production).ThenInclude(x => x.Client).Include(x => x.ProductionProcess).ThenInclude(x => x.Production).ThenInclude(x => x.PieceModel).Include(x => x.ProductionProcess).ThenInclude(x => x.ServiceProcess).AsQueryable();
        if (startDate.HasValue) query = query.Where(x => x.ProductionProcess.Production.ProductionDate >= startDate.Value.Date);
        if (endDate.HasValue)
        {
            query = endDate.Value.Date < DateTime.MaxValue.Date
                ? query.Where(x => x.ProductionProcess.Production.ProductionDate < endDate.Value.Date.AddDays(1))
                : query.Where(x => x.ProductionProcess.Production.ProductionDate <= endDate.Value.Date);
        }
        if (seamstressId.HasValue) query = query.Where(x => x.SeamstressId == seamstressId.Value);
        var assignments = await query.OrderByDescending(x => x.ProductionProcess.Production.ProductionDate).ToListAsync();
        var rows = assignments.Select(x => new RelatorioRowVm(x.Seamstress.Name, x.ProductionProcess.Production.Client.Name, x.ProductionProcess.Production.PieceModel.Name, x.ProductionProcess.ServiceProcess.Name, x.ProducedQuantity, x.PricePerPiece, x.ProducedQuantity * x.PricePerPiece, x.ProductionProcess.Production.ProductionDate, x.ProductionProcess.Production.DeliveryDate)).ToList();
        return View(new RelatorioFilterVm { StartDate = startDate, EndDate = endDate, SeamstressId = seamstressId, Seamstresses = await ActiveSeamstressesAsync(), Rows = rows, TotalProduced = rows.Sum(x => x.Quantity), TotalAmount = rows.Sum(x => x.Total) });
    }

    [HttpGet]
    public async Task<IActionResult> Monthly(int? year, int? month)
    {
        var now = DateTime.Today;
        var selectedYear = year is >= 2000 and <= 2100 ? year.Value : now.Year;
        var selectedMonth = month is >= 1 and <= 12 ? month.Value : now.Month;
        var start = new DateTime(selectedYear, selectedMonth, 1);
        var end = start.AddMonths(1);
        var assignments = await db.Assignments.AsNoTracking().Include(x => x.Seamstress).Include(x => x.ProductionProcess).ThenInclude(x => x.Production).ThenInclude(x => x.Client).Where(x => x.ProductionProcess.Production.ProductionDate >= start && x.ProductionProcess.Production.ProductionDate < end).ToListAsync();
        var values = assignments.Select(x => new { x.Seamstress.Name, Client = x.ProductionProcess.Production.Client.Name, Quantity = x.ProducedQuantity, Amount = x.ProducedQuantity * x.PricePerPiece }).ToList();
        return View(new RelatorioMensalVm { Year = selectedYear, Month = selectedMonth, TotalProduced = values.Sum(x => x.Quantity), TotalAmount = values.Sum(x => x.Amount), BySeamstress = values.GroupBy(x => x.Name).Select(g => new ResumoCostureiraVm(g.Key, g.Sum(x => x.Quantity), g.Sum(x => x.Amount))).OrderByDescending(x => x.Amount).ToList(), ByClient = values.GroupBy(x => x.Client).Select(g => new ResumoClienteVm(g.Key, g.Sum(x => x.Quantity), g.Sum(x => x.Amount))).OrderByDescending(x => x.Amount).ToList() });
    }

    private async Task<IReadOnlyList<LookupVm>> ActiveSeamstressesAsync() => await db.Seamstresses.AsNoTracking().Where(x => x.Active).OrderBy(x => x.Name).Select(x => new LookupVm(x.Id, x.Name)).ToListAsync();
}