using CosturaProducao.Data;
using CosturaProducao.Models;
using CosturaProducao.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosturaProducao.Controllers;

public sealed class ProducoesController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await db.Productions
            .AsNoTracking()
            .Include(x => x.Client)
            .Include(x => x.PieceModel)
            .Include(x => x.PieceVariant)
            .OrderBy(x => x.DeliveryDate)
            .Select(x => new ProducaoRowVm(
                x.Id,
                x.Client.Name,
                x.PieceModel.Name,
                x.PieceVariant.Cor,
                x.PieceVariant.Tamanho,
                x.TotalQuantity,
                x.DeliveryDate,
                x.Status.ToString()))
            .ToListAsync();

        return View(new ProducaoIndexVm
        {
            Items = items
        });
    }


    // ============================================================
    // NOVA PRODUÇÃO
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(await BuildFormAsync(new ProducaoCreateVm()));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProducaoCreateVm input)
    {
        // --------------------------------------------------------
        // DATA
        // --------------------------------------------------------

        if (input.DeliveryDate.Date < input.ProductionDate.Date)
        {
            ModelState.AddModelError(
                nameof(input.DeliveryDate),
                "A entrega não pode ser anterior à produção.");
        }


        // --------------------------------------------------------
        // CLIENTE
        // --------------------------------------------------------

        var clientExists = await db.Clients
            .AnyAsync(x =>
                x.Id == input.ClientId &&
                x.Active);

        if (!clientExists)
        {
            ModelState.AddModelError(
                nameof(input.ClientId),
                "Selecione um cliente ativo.");
        }


        // --------------------------------------------------------
        // PEÇA
        // --------------------------------------------------------

        var pieceExists = await db.PieceModels
            .AnyAsync(x =>
                x.Id == input.PieceModelId &&
                x.Active);

        if (!pieceExists)
        {
            ModelState.AddModelError(
                nameof(input.PieceModelId),
                "Selecione uma peça ativa.");
        }


        // --------------------------------------------------------
        // VARIAÇÃO
        // --------------------------------------------------------

        PieceVariant? variant = null;

        if (input.PieceVariantId <= 0)
        {
            ModelState.AddModelError(
                nameof(input.PieceVariantId),
                "Selecione uma cor e tamanho.");
        }
        else
        {
            variant = await db.PieceVariants
                .FirstOrDefaultAsync(x =>
                    x.Id == input.PieceVariantId &&
                    x.PieceModelId == input.PieceModelId &&
                    x.Active);
        }

        if (input.PieceVariantId > 0 && variant is null)
        {
            ModelState.AddModelError(
                nameof(input.PieceVariantId),
                "A cor e o tamanho selecionados não pertencem a esta peça.");
        }


        // --------------------------------------------------------
        // QUANTIDADE
        // --------------------------------------------------------

        if (input.TotalQuantity <= 0)
        {
            ModelState.AddModelError(
                nameof(input.TotalQuantity),
                "Informe uma quantidade maior que zero.");
        }


        // --------------------------------------------------------
        // PROCESSOS
        // --------------------------------------------------------

        var selectedServiceIds =
            input.ServiceProcessIds ?? new List<int>();

        var validServiceIds = await db.ServiceProcesses
            .Where(x =>
                x.Active &&
                selectedServiceIds.Contains(x.Id))
            .ToListAsync();

        if (validServiceIds.Count == 0)
        {
            ModelState.AddModelError(
                nameof(input.ServiceProcessIds),
                "Selecione pelo menos um processo.");
        }


        // --------------------------------------------------------
        // SE EXISTIR ERRO
        // --------------------------------------------------------

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(input));
        }


        // --------------------------------------------------------
        // CRIAÇÃO
        // --------------------------------------------------------

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        try
        {
            var production = new Production
            {
                ClientId = input.ClientId,

                PieceModelId = input.PieceModelId,

                PieceVariantId = variant!.Id,

                TotalQuantity = input.TotalQuantity,

                ProductionDate =
                    input.ProductionDate.Date,

                DeliveryDate =
                    input.DeliveryDate.Date,

                Notes = input.Notes?.Trim(),

                Status = ProductionStatus.Pending
            };


            foreach (var service in validServiceIds)
            {
                production.Processes.Add(
                    new ProductionProcess
                    {
                        ServiceProcessId = service.Id,

                        PricePerPiece =
                            service.DefaultPricePerPiece
                    });
            }


            db.Productions.Add(production);

            await db.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }


        TempData["Success"] =
            "Produção criada com sucesso.";

        return RedirectToAction(nameof(Index));
    }


    // ============================================================
    // BUSCAR VARIAÇÕES DA PEÇA
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Variantes(int pieceModelId)
    {
        if (pieceModelId <= 0)
        {
            return BadRequest();
        }

        var variants = await db.PieceVariants
            .AsNoTracking()
            .Where(x =>
                x.PieceModelId == pieceModelId &&
                x.Active)
            .OrderBy(x => x.Cor)
            .ThenBy(x => x.Tamanho)
            .Select(x => new
            {
                id = x.Id,
                color = x.Cor,
                size = x.Tamanho
            })
            .ToListAsync();

        return Json(variants);
    }


    // ============================================================
    // FORMULÁRIO
    // ============================================================

    private async Task<ProducaoCreateVm> BuildFormAsync(
        ProducaoCreateVm input)
    {
        var model = new ProducaoCreateVm
        {
            ClientId = input.ClientId,

            PieceModelId = input.PieceModelId,

            PieceVariantId = input.PieceVariantId,

            TotalQuantity = input.TotalQuantity,

            ProductionDate =
                input.ProductionDate == default
                    ? DateTime.Today
                    : input.ProductionDate,

            DeliveryDate =
                input.DeliveryDate == default
                    ? DateTime.Today.AddDays(7)
                    : input.DeliveryDate,

            Notes = input.Notes,

            ServiceProcessIds =
                input.ServiceProcessIds ?? new List<int>(),

            Clients = await db.Clients
                .AsNoTracking()
                .Where(x => x.Active)
                .OrderBy(x => x.Name)
                .Select(x =>
                    new LookupVm(
                        x.Id,
                        x.Name))
                .ToListAsync(),

            Pieces = await db.PieceModels
                .AsNoTracking()
                .Where(x => x.Active)
                .OrderBy(x => x.Name)
                .Select(x =>
                    new LookupVm(
                        x.Id,
                        $"{x.Name} · {x.Code}"))
                .ToListAsync(),

            Variants = await db.PieceVariants
                .AsNoTracking()
                .Where(x =>
                    x.PieceModelId ==
                    input.PieceModelId &&
                    x.Active)
                .OrderBy(x => x.Cor)
                .ThenBy(x => x.Tamanho)
                .Select(x =>
                    new PieceVariantLookupVm(
                        x.Id,
                        x.Tamanho,
                        x.Cor))
                .ToListAsync(),

            Services = await db.ServiceProcesses
                .AsNoTracking()
                .Where(x => x.Active)
                .OrderBy(x => x.Name)
                .Select(x =>
                    new ServiceLookupVm(
                        x.Id,
                        x.Name,
                        x.DefaultPricePerPiece))
                .ToListAsync()
        };

        return model;
    }
}