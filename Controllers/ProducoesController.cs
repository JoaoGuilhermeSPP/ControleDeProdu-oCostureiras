using CosturaProducao.Data;
using CosturaProducao.Models;
using CosturaProducao.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosturaProducao.Controllers;

[Authorize]

public sealed class ProducoesController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await db.Productions.AsNoTracking()
            .Include(x => x.Client).Include(x => x.PieceModel)
            .OrderBy(x => x.DeliveryDate)
            .Select(x => new ProducaoRowVm(x.Id, x.Client.Name, x.PieceModel.Name, x.Color, x.TotalQuantity, x.DeliveryDate, x.Status.ToString()))
            .ToListAsync();
        return View(new ProducaoIndexVm { Items = items });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(await BuildFormAsync(new ProducaoCreateVm()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProducaoCreateVm input)
    {
        if (input.DeliveryDate.Date < input.ProductionDate.Date)
            ModelState.AddModelError(nameof(input.DeliveryDate), "A entrega não pode ser anterior à produção.");
        if (!await db.Clients.AnyAsync(x => x.Id == input.ClientId && x.Active))
            ModelState.AddModelError(nameof(input.ClientId), "Selecione um cliente ativo.");
        if (!await db.PieceModels.AnyAsync(x => x.Id == input.PieceModelId && x.Active))
            ModelState.AddModelError(nameof(input.PieceModelId), "Selecione um modelo ativo.");
        var validServiceIds = await db.ServiceProcesses.Where(x => x.Active && input.ServiceProcessIds.Contains(x.Id)).ToListAsync();
        if (validServiceIds.Count == 0) ModelState.AddModelError(nameof(input.ServiceProcessIds), "Selecione pelo menos um processo.");
        if (!ModelState.IsValid) return View(await BuildFormAsync(input));

        await using var transaction = await db.Database.BeginTransactionAsync();
        var production = new Production
        {
            ClientId = input.ClientId, PieceModelId = input.PieceModelId, Color = input.Color.Trim(),
            TotalQuantity = input.TotalQuantity, ProductionDate = input.ProductionDate.Date,
            DeliveryDate = input.DeliveryDate.Date, Notes = input.Notes?.Trim()
        };
        foreach (var service in validServiceIds)
            production.Processes.Add(new ProductionProcess { ServiceProcessId = service.Id, PricePerPiece = service.DefaultPricePerPiece });
        db.Productions.Add(production);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["Success"] = "Produção criada com os preços atuais dos processos.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ProducaoCreateVm> BuildFormAsync(ProducaoCreateVm input)
    {
        return new ProducaoCreateVm
        {
            ClientId = input.ClientId, PieceModelId = input.PieceModelId, Color = input.Color,
            TotalQuantity = input.TotalQuantity, ProductionDate = input.ProductionDate == default ? DateTime.Today : input.ProductionDate,
            DeliveryDate = input.DeliveryDate == default ? DateTime.Today.AddDays(7) : input.DeliveryDate,
            Notes = input.Notes, ServiceProcessIds = input.ServiceProcessIds,
            Clients = await db.Clients.AsNoTracking().Where(x => x.Active).OrderBy(x => x.Name).Select(x => new LookupVm(x.Id, x.Name)).ToListAsync(),
            Pieces = await db.PieceModels.AsNoTracking().Where(x => x.Active).OrderBy(x => x.Name).Select(x => new LookupVm(x.Id, $"{x.Name} · {x.Code}")).ToListAsync(),
            Services = await db.ServiceProcesses.AsNoTracking().Where(x => x.Active).OrderBy(x => x.Name).Select(x => new ServiceLookupVm(x.Id, x.Name, x.DefaultPricePerPiece)).ToListAsync()
        };
    }
}