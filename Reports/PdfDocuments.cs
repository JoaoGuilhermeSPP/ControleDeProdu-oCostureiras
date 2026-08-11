using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CosturaProducao.Reports;

internal static class PdfStyle
{
    public static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
    public static void Header(IContainer container, string text) => container.Background("174A5B").Padding(7).Text(text).FontColor(Colors.White).Bold();
    public static void Cell(IContainer container, string text) => container.BorderBottom(0.5f).BorderColor("CBD1CC").Padding(7).Text(text);
}

public sealed class FichaPdfDocument(FichaPdfModel model) : IDocument
{
    public DocumentMetadata GetMetadata() => new() { Title = $"Ficha de produção - {model.Piece}", Author = "Oficina 01" };
    public void Compose(IDocumentContainer document) => document.Page(page =>
    {
        page.Size(PageSizes.A4); page.Margin(1.4f, Unit.Centimetre); page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));
        page.Header().Column(c => { c.Item().Text("FICHA DE PRODUÇÃO").FontSize(18).Bold(); c.Item().Text($"Costureira: {model.Seamstress}  ·  Entrega: {model.DeliveryDate:dd/MM/yyyy}"); c.Item().PaddingTop(6).LineHorizontal(1); });
        page.Content().PaddingVertical(14).Column(c =>
        {
            c.Spacing(10);
            c.Item().Table(t => { t.ColumnsDefinition(x => { x.RelativeColumn(); x.RelativeColumn(); }); PdfStyle.Header(t.Cell(), "INFORMAÇÕES"); PdfStyle.Header(t.Cell(), "GABARITO"); PdfStyle.Cell(t.Cell(), $"Cliente: {model.Client}\nModelo: {model.Piece}\nCor: {model.Color}\nQuantidade atribuída: {model.Quantity}\nServiço: {model.Service}\nValor por peça: R$ {model.Price.ToString("N2", PdfStyle.PtBr)}\nTotal previsto: R$ {model.Total.ToString("N2", PdfStyle.PtBr)}"); if (!string.IsNullOrWhiteSpace(model.TemplatePath) && File.Exists(model.TemplatePath)) t.Cell().Padding(8).Image(model.TemplatePath).FitArea(); else PdfStyle.Cell(t.Cell(), "Sem imagem de gabarito"); });
            c.Item().Text("Observações").Bold(); c.Item().MinHeight(70).Border(1).BorderColor("CBD1CC").Padding(8).Text(model.Notes ?? "");
        });
        page.Footer().AlignCenter().Text(x => { x.Span("Oficina 01  ·  Página "); x.CurrentPageNumber(); x.Span(" de "); x.TotalPages(); });
    });
}

public sealed class TemplatePdfDocument(TemplatePdfModel model) : IDocument
{
    public DocumentMetadata GetMetadata() => new() { Title = $"Gabarito - {model.Name}", Author = "Oficina 01" };
    public void Compose(IDocumentContainer document) => document.Page(page =>
    {
        page.Size(PageSizes.A4); page.Margin(1.2f, Unit.Centimetre); page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));
        page.Header().Text($"GABARITO DA PEÇA  ·  {model.Code}").FontSize(17).Bold();
        page.Content().PaddingVertical(12).Column(c => { c.Spacing(8); c.Item().Text(model.Name).FontSize(14).Bold(); c.Item().Text($"Cor: {model.Color ?? "Não informada"}"); c.Item().Text(model.Description ?? ""); if (!string.IsNullOrWhiteSpace(model.TemplatePath) && File.Exists(model.TemplatePath)) c.Item().Border(1).BorderColor("CBD1CC").Padding(10).AlignCenter().Image(model.TemplatePath).FitArea(); else c.Item().MinHeight(500).Border(1).BorderColor("CBD1CC").AlignCenter().AlignMiddle().Text("Sem imagem de gabarito"); });
        page.Footer().AlignCenter().Text("Documento para impressão · Oficina 01");
    });
}

public sealed class MonthlyPdfDocument(MonthlyPdfModel model) : IDocument
{
    public DocumentMetadata GetMetadata() => new() { Title = $"Relatório mensal - {model.Month:00}/{model.Year}", Author = "Oficina 01" };
    public void Compose(IDocumentContainer document) => document.Page(page =>
    {
        page.Size(PageSizes.A4); page.Margin(1.3f, Unit.Centimetre); page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));
        page.Header().Text($"RELATÓRIO MENSAL  ·  {model.Month:00}/{model.Year}").FontSize(17).Bold();
        page.Content().PaddingVertical(12).Column(c => { c.Spacing(14); c.Item().Text($"Peças produzidas: {model.TotalProduced:N0}   ·   Total: R$ {model.TotalAmount.ToString("N2", PdfStyle.PtBr)}").FontSize(12).Bold(); AddSummary(c, "POR COSTUREIRA", model.BySeamstress); AddSummary(c, "POR CLIENTE", model.ByClient); });
        page.Footer().AlignCenter().Text(x => { x.Span("Oficina 01  ·  Página "); x.CurrentPageNumber(); });
    });
    private static void AddSummary(ColumnDescriptor column, string title, IReadOnlyList<MonthlyPdfRow> rows) { column.Item().Text(title).Bold(); column.Item().Table(t => { t.ColumnsDefinition(x => { x.RelativeColumn(2); x.RelativeColumn(); x.RelativeColumn(); }); PdfStyle.Header(t.Cell(), "Nome"); PdfStyle.Header(t.Cell(), "Peças"); PdfStyle.Header(t.Cell(), "Valor"); foreach (var row in rows) { PdfStyle.Cell(t.Cell(), row.Name); PdfStyle.Cell(t.Cell(), row.Quantity.ToString("N0")); PdfStyle.Cell(t.Cell(), $"R$ {row.Amount.ToString("N2", PdfStyle.PtBr)}"); } }); }
}