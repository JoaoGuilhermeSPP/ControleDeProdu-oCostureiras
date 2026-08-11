namespace CosturaProducao.Reports;

public sealed record FichaPdfModel(string Seamstress, string Client, string Piece, string Color, int Quantity, string Service, decimal Price, decimal Total, DateTime DeliveryDate, string? Notes, string? TemplatePath);
public sealed record TemplatePdfModel(string Name, string Code, string? Color, string? Description, string? TemplatePath);
public sealed record MonthlyPdfModel(int Year, int Month, int TotalProduced, decimal TotalAmount, IReadOnlyList<MonthlyPdfRow> BySeamstress, IReadOnlyList<MonthlyPdfRow> ByClient);
public sealed record MonthlyPdfRow(string Name, int Quantity, decimal Amount);