using QuestPDF.Fluent;

namespace CosturaProducao.Reports;

public interface IPdfService
{
    byte[] Assignment(FichaPdfModel model);
    byte[] Template(TemplatePdfModel model);
    byte[] Monthly(MonthlyPdfModel model);
}

public sealed class PdfService : IPdfService
{
    public byte[] Assignment(FichaPdfModel model) => new FichaPdfDocument(model).GeneratePdf();
    public byte[] Template(TemplatePdfModel model) => new TemplatePdfDocument(model).GeneratePdf();
    public byte[] Monthly(MonthlyPdfModel model) => new MonthlyPdfDocument(model).GeneratePdf();
}