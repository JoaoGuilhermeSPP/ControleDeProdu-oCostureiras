using CosturaProducao.Data;
using CosturaProducao.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CosturaProducao.Controllers;

public sealed class PdfController(
    ApplicationDbContext db,
    IPdfService pdf,
    IWebHostEnvironment environment) : Controller
{
    // =========================================================
    // PDF DA DISTRIBUIÇÃO
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Assignment(int id)
    {
        var item = await db.Assignments
            .AsNoTracking()

            // Costureira
            .Include(x => x.Seamstress)

            // Processo
            .Include(x => x.ProductionProcess)
                .ThenInclude(x => x.ServiceProcess)

            // Produção
            .Include(x => x.ProductionProcess)
                .ThenInclude(x => x.Production)
                    .ThenInclude(x => x.Client)

            // Modelo da peça
            .Include(x => x.ProductionProcess)
                .ThenInclude(x => x.Production)
                    .ThenInclude(x => x.PieceModel)

            // Variante da peça
            .Include(x => x.ProductionProcess)
                .ThenInclude(x => x.Production)
                    .ThenInclude(x => x.PieceVariant)

            .SingleOrDefaultAsync(x => x.Id == id);

        if (item is null)
            return NotFound();

        var production = item.ProductionProcess.Production;

        var variant = production.PieceVariant;

        var model = new FichaPdfModel(
            item.Seamstress.Name,

            production.Client.Name,

            production.PieceModel.Name,

            // COR
            variant.Cor,

            item.PlannedQuantity,

            item.ProductionProcess.ServiceProcess.Name,

            item.PricePerPiece,

            item.TotalAmount,

            production.DeliveryDate,

            production.Notes,

            ResolveTemplate(
                production.PieceModel.TemplateImagePath)
        );

        var arquivo = pdf.Assignment(model);

        return File(
            arquivo,
            "application/pdf",
            $"ficha-distribuicao-{id}.pdf");
    }


    // =========================================================
    // PDF DO GABARITO DA PEÇA
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Template(int id)
    {
        var piece = await db.PieceModels
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

        if (piece is null)
            return NotFound();

        /*
         * Como Cor foi retirada de PieceModel,
         * não devemos mais usar:
         *
         * piece.Color
         *
         * O gabarito agora representa o modelo da peça.
         */

        var model = new TemplatePdfModel(
            piece.Name,
            piece.Code,
            null,
            piece.Description,
            ResolveTemplate(
                piece.TemplateImagePath)
        );

        var arquivo = pdf.Template(model);

        return File(
            arquivo,
            "application/pdf",
            $"gabarito-{piece.Code}.pdf");
    }


    // =========================================================
    // RELATÓRIO MENSAL
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Monthly(
        int? year,
        int? month)
    {
        var today = DateTime.Today;

        var selectedYear =
            year is >= 2000 and <= 2100
                ? year.Value
                : today.Year;

        var selectedMonth =
            month is >= 1 and <= 12
                ? month.Value
                : today.Month;

        var start =
            new DateTime(
                selectedYear,
                selectedMonth,
                1);

        var end =
            start.AddMonths(1);


        var assignments = await db.Assignments
            .AsNoTracking()

            // Costureira
            .Include(x => x.Seamstress)

            // Processo
            .Include(x => x.ProductionProcess)

                // Produção
                .ThenInclude(x => x.Production)

                    // Cliente
                    .ThenInclude(x => x.Client)

            .Where(x =>
                x.ProductionProcess
                    .Production
                    .ProductionDate >= start

                &&

                x.ProductionProcess
                    .Production
                    .ProductionDate < end)

            .ToListAsync();


        // =====================================================
        // VALORES
        // =====================================================

        var values = assignments
            .Select(x => new
            {
                Name = x.Seamstress.Name,

                Client =
                    x.ProductionProcess
                        .Production
                        .Client
                        .Name,

                Quantity =
                    x.ProducedQuantity,

                Amount =
                    x.ProducedQuantity *
                    x.PricePerPiece
            })
            .ToList();


        // =====================================================
        // RELATÓRIO
        // =====================================================

        var model = new MonthlyPdfModel(

            selectedYear,

            selectedMonth,

            values.Sum(x => x.Quantity),

            values.Sum(x => x.Amount),

            // Por costureira
            values
                .GroupBy(x => x.Name)

                .Select(g =>
                    new MonthlyPdfRow(
                        g.Key,
                        g.Sum(x => x.Quantity),
                        g.Sum(x => x.Amount)))

                .OrderByDescending(
                    x => x.Amount)

                .ToList(),

            // Por cliente
            values
                .GroupBy(x => x.Client)

                .Select(g =>
                    new MonthlyPdfRow(
                        g.Key,
                        g.Sum(x => x.Quantity),
                        g.Sum(x => x.Amount)))

                .OrderByDescending(
                    x => x.Amount)

                .ToList()
        );


        var arquivo =
            pdf.Monthly(model);


        return File(
            arquivo,
            "application/pdf",
            $"relatorio-{selectedYear}-{selectedMonth:00}.pdf");
    }


    // =========================================================
    // LOCALIZAÇÃO DO GABARITO
    // =========================================================

    private string? ResolveTemplate(
        string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;


        var root =
            Path.GetFullPath(
                environment.WebRootPath);


        var full =
            Path.GetFullPath(
                Path.Combine(
                    root,
                    relativePath
                        .TrimStart('/', '\\')));


        var isInsideWebRoot =
            full.StartsWith(
                root + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase);


        if (!isInsideWebRoot)
            return null;


        if (!System.IO.File.Exists(full))
            return null;


        return full;
    }
}