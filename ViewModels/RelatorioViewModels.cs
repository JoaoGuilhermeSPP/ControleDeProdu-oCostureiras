namespace CosturaProducao.ViewModels;

public sealed class RelatorioFilterVm
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? SeamstressId { get; set; }
    public IReadOnlyList<LookupVm> Seamstresses { get; init; } = [];
    public IReadOnlyList<RelatorioRowVm> Rows { get; init; } = [];
    public int TotalProduced { get; init; }
    public decimal TotalAmount { get; init; }
}

public sealed record RelatorioRowVm(string Seamstress, string Client, string Piece, string Service, int Quantity, decimal Price, decimal Total, DateTime Date, DateTime DeliveryDate);
public sealed record ResumoCostureiraVm(string Seamstress, int Quantity, decimal Amount);
public sealed record ResumoClienteVm(string Client, int Quantity, decimal Amount);

public sealed class RelatorioMensalVm
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int TotalProduced { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<ResumoCostureiraVm> BySeamstress { get; init; } = [];
    public IReadOnlyList<ResumoClienteVm> ByClient { get; init; } = [];
}

public sealed class DashboardVm
{
    public int PendingProductions { get; init; }
    public int ActiveProductions { get; init; }
    public int CompletedProductions { get; init; }
    public int ProducedThisMonth { get; init; }
    public int UpcomingDeliveries { get; init; }
    public decimal AmountThisMonth { get; init; }
}