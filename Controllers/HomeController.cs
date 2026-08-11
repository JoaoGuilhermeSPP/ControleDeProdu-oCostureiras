using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CosturaProducao.Data;
using CosturaProducao.Models;
using CosturaProducao.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CosturaProducao.Controllers;

public sealed class HomeController(ApplicationDbContext db) : Controller
{
    public IActionResult Index() => View();
    [Authorize]

    public async Task<IActionResult> Dashboard()
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var pending = await db.Productions.CountAsync(x => x.Status == ProductionStatus.Pending);
        var active = await db.Productions.CountAsync(x => x.Status == ProductionStatus.InProgress);
        var completed = await db.Productions.CountAsync(x => x.Status == ProductionStatus.Completed);
        var upcoming = await db.Productions.CountAsync(x => x.DeliveryDate.Date >= today && x.DeliveryDate.Date <= today.AddDays(7));
        var monthlyAssignments = db.Assignments.Where(x => x.ProductionProcess.Production.ProductionDate >= monthStart);
        return View(new DashboardVm
        {
            PendingProductions = pending, ActiveProductions = active, CompletedProductions = completed, UpcomingDeliveries = upcoming,
            ProducedThisMonth = await monthlyAssignments.SumAsync(x => (int?)x.ProducedQuantity) ?? 0,
            AmountThisMonth = await monthlyAssignments.SumAsync(x => (decimal?)(x.ProducedQuantity * x.PricePerPiece)) ?? 0
        });
    }
}