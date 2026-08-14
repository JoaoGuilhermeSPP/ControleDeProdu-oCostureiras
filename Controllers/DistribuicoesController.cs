using CosturaProducao.Data;
using CosturaProducao.Models;
using CosturaProducao.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosturaProducao.Controllers;


public sealed class DistribuicoesController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string filtro = "todos")
    {
        if (filtro != "todos" &&
            filtro != "andamento" &&
            filtro != "finalizadas")
        {
            filtro = "todos";
        }

        var productions = await db.Productions
            .AsNoTracking()
            .Include(x => x.Client)
            .Include(x => x.PieceModel)
            .Include(x => x.Processes)
                .ThenInclude(x => x.ServiceProcess)
            .Include(x => x.Processes)
                .ThenInclude(x => x.Assignments)
                    .ThenInclude(x => x.Seamstress)
            .OrderBy(x => x.DeliveryDate)
            .ToListAsync();

        var productionViewModels = productions
            .Select(p =>
            {
                var processes = p.Processes
                    .Select(process =>
                        new DistribuicaoProcessVm(
                            process.Id,
                            process.ServiceProcess.Name,
                            process.PricePerPiece,
                            process.Assignments.Sum(x => x.PlannedQuantity),
                            process.Assignments.Sum(x => x.ProducedQuantity),
                            ProcessStatus(process, p.TotalQuantity),
                            process.Assignments
                                .Select(a =>
                                    new AssignmentVm(
                                        a.Id,
                                        a.Seamstress.Name,
                                        a.PlannedQuantity,
                                        a.ProducedQuantity,
                                        a.PricePerPiece,
                                        a.TotalAmount,
                                        a.Status.ToString()))
                                .ToList()))
                    .ToList();

                return new
                {
                    Production = new DistribuicaoProductionVm(
                        p.Id,
                        p.Client.Name,
                        p.PieceModel.Name,
                        p.TotalQuantity,
                        processes),

                    Finalizada = IsProductionCompleted(p)
                };
            });

        if (filtro == "finalizadas")
        {
            productionViewModels =
                productionViewModels.Where(x => x.Finalizada);
        }
        else if (filtro == "andamento")
        {
            productionViewModels =
                productionViewModels.Where(x => !x.Finalizada);
        }

        var model = new DistribuicaoIndexVm
        {
            Filtro = filtro,
            Productions = productionViewModels
                .Select(x => x.Production)
                .ToList()
        };

        return View(model);
    }
    private static bool IsProductionCompleted(Production production)
    {
        if (production.Processes.Count == 0)
            return false;

        foreach (var process in production.Processes)
        {
            var produced = process.Assignments
                .Sum(x => x.ProducedQuantity);

            if (produced < production.TotalQuantity)
                return false;
        }

        return true;
    }
    [HttpGet]
    public async Task<IActionResult> Create(int productionProcessId)
    {
        var process = await db.ProductionProcesses.Include(x => x.Production).ThenInclude(x => x.PieceModel).Include(x => x.ServiceProcess).FirstOrDefaultAsync(x => x.Id == productionProcessId);
        if (process is null) return NotFound();
        return View(new DistribuicaoCreateVm { ProductionId = process.ProductionId, ProductionProcessId = process.Id, PricePerPiece = process.PricePerPiece, ProductionDescription = $"{process.Production.PieceModel.Name} · {process.ServiceProcess.Name}", Seamstresses = await ActiveSeamstressesAsync() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DistribuicaoCreateVm input)
    {
        // Busca o processo de produção
        var process = await db.ProductionProcesses
            .Include(x => x.Production)
            .FirstOrDefaultAsync(x => x.Id == input.ProductionProcessId);

        if (process is null)
            return NotFound();


        // Quantidade que já foi distribuída para este processo
        var quantidadeDistribuida = await db.Assignments
            .Where(x => x.ProductionProcessId == process.Id)
            .SumAsync(x => (int?)x.PlannedQuantity) ?? 0;


        // Quantidade que ainda pode ser distribuída
        var quantidadeRestante =
            process.Production.TotalQuantity - quantidadeDistribuida;


        // Validação da quantidade
        if (input.PlannedQuantity <= 0)
        {
            ModelState.AddModelError(
                nameof(input.PlannedQuantity),
                "Informe uma quantidade maior que zero."
            );
        }

        if (input.PlannedQuantity > quantidadeRestante)
        {
            ModelState.AddModelError(
                nameof(input.PlannedQuantity),
                $"Restam apenas {quantidadeRestante} peças para este processo."
            );
        }


        // Verifica se a costureira existe e está ativa
        var costureiraExiste = await db.Seamstresses
            .AnyAsync(x =>
                x.Id == input.SeamstressId &&
                x.Active);

        if (!costureiraExiste)
        {
            ModelState.AddModelError(
                nameof(input.SeamstressId),
                "Selecione uma costureira ativa."
            );
        }


        // Se houver algum erro, retorna para o formulário
        if (!ModelState.IsValid)
        {
            input.PricePerPiece = process.PricePerPiece;

            input.ProductionDescription =
                $"{process.Production.PieceModel?.Name} · Distribuição";

            input.Seamstresses =
                await ActiveSeamstressesAsync();

            return View(input);
        }


        // Cria a distribuição
        var assignment = new Assignment
        {
            ProductionProcessId = process.Id,
            SeamstressId = input.SeamstressId,
            PlannedQuantity = input.PlannedQuantity,
            ProducedQuantity = 0,
            PricePerPiece = process.PricePerPiece,
            Status = AssignmentStatus.Pending
        };


        db.Assignments.Add(assignment);


        // Salva no MySQL
        await db.SaveChangesAsync();


        // Mensagem para aparecer na tela de distribuição
        TempData["Success"] =
            "Distribuição realizada com sucesso.";


        // Volta para a tela de distribuição
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProgress(ProgressoVm input)
    {
        var assignment = await db.Assignments.Include(x => x.ProductionProcess).FirstOrDefaultAsync(x => x.Id == input.AssignmentId);
        if (assignment is null) return NotFound();
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Informe uma quantidade produzida válida.";
            return RedirectToAction(nameof(Index));
        }
        if (input.ProducedQuantity > assignment.PlannedQuantity)
        {
            TempData["Error"] = "A quantidade produzida não pode superar a quantidade planejada.";
            return RedirectToAction(nameof(Index));
        }
        assignment.ProducedQuantity = input.ProducedQuantity;
        assignment.Status = input.ProducedQuantity == 0
            ? AssignmentStatus.Pending
            : input.ProducedQuantity == assignment.PlannedQuantity
                ? AssignmentStatus.Completed
                : assignment.Status == AssignmentStatus.Pending ? AssignmentStatus.InProgress : AssignmentStatus.Partial;
        var allAssignments = await db.Assignments.Where(x => x.ProductionProcess.ProductionId == assignment.ProductionProcess.ProductionId).ToListAsync();
        var processCount = await db.ProductionProcesses.CountAsync(x => x.ProductionId == assignment.ProductionProcess.ProductionId);
        var production = await db.Productions.FindAsync(assignment.ProductionProcess.ProductionId);
        if (production is not null)
            production.Status = processCount > 0 && allAssignments.Count > 0 && allAssignments.All(x => x.Status == AssignmentStatus.Completed)
                ? ProductionStatus.Completed
                : allAssignments.Any(x => x.ProducedQuantity > 0) ? ProductionStatus.InProgress : ProductionStatus.Pending;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<IReadOnlyList<LookupVm>> ActiveSeamstressesAsync() => await db.Seamstresses.AsNoTracking().Where(x => x.Active).OrderBy(x => x.Name).Select(x => new LookupVm(x.Id, x.Name)).ToListAsync();
    private static string ProcessStatus(ProductionProcess process, int productionQuantity) => process.Assignments.Sum(x => x.ProducedQuantity) >= productionQuantity ? "Completed" : process.Assignments.Sum(x => x.ProducedQuantity) > 0 ? "Partial" : "Pending";
}