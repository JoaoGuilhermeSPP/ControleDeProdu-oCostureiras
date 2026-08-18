using Microsoft.AspNetCore.Mvc;
using CosturaProducao.Data;
using CosturaProducao.Models;
using CosturaProducao.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CosturaProducao.Controllers;

public sealed class HomeController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var today = DateTime.Today;

        var monthStart = new DateTime(
            today.Year,
            today.Month,
            1);

        var pending = await db.Productions
            .CountAsync(x =>
                x.Status == ProductionStatus.Pending);

        var active = await db.Productions
            .CountAsync(x =>
                x.Status == ProductionStatus.InProgress);

        var completed = await db.Productions
            .CountAsync(x =>
                x.Status == ProductionStatus.Completed);

        var upcoming = await db.Productions
            .CountAsync(x =>
                x.DeliveryDate >= today &&
                x.DeliveryDate < today.AddDays(8));


        /*
         * Produções/processos do mês atual
         */
        var monthlyAssignments = db.Assignments
            .AsNoTracking()
            .Where(x =>
                x.ProductionProcess
                    .Production
                    .ProductionDate >= monthStart);


        /*
         * Quantidade produzida
         *
         * SQLite consegue fazer SUM de inteiros normalmente.
         */
        var producedThisMonth =
            await monthlyAssignments
                .Select(x => x.ProducedQuantity)
                .ToListAsync();

        var totalProduced =
            producedThisMonth.Sum();


        /*
         * Valores monetários
         *
         * NÃO fazemos SumAsync(decimal) no SQLite.
         *
         * Primeiro trazemos os valores para memória.
         */
        var monthlyAmounts =
            await monthlyAssignments
                .Select(x =>
                    new
                    {
                        x.ProducedQuantity,
                        x.PricePerPiece
                    })
                .ToListAsync();


        /*
         * Agora o cálculo é feito em C#,
         * fora do SQLite.
         */
        var amountThisMonth =
            monthlyAmounts.Sum(x =>
                x.ProducedQuantity *
                x.PricePerPiece);


        /*
         * Monta o ViewModel
         */
        var model = new DashboardVm
        {
            PendingProductions = pending,

            ActiveProductions = active,

            CompletedProductions = completed,

            UpcomingDeliveries = upcoming,

            ProducedThisMonth = totalProduced,

            AmountThisMonth = amountThisMonth
        };


        return View(model);
    }
}