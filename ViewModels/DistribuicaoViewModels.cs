using System.ComponentModel.DataAnnotations;

namespace CosturaProducao.ViewModels;

public sealed class DistribuicaoIndexVm
{
    public string Filtro { get; init; } = "todos";

    public IReadOnlyList<DistribuicaoProductionVm> Productions { get; init; } = [];
}

public sealed record DistribuicaoProductionVm(
    int Id,
    string Client,
    string Piece,
    string Color,
    string Size,
    int Quantity,
    List<DistribuicaoProcessVm> Processes
);
public sealed record DistribuicaoProcessVm(int Id, string Name, decimal Price, int Planned, int Produced, string Status, IReadOnlyList<AssignmentVm> Assignments);
public sealed record AssignmentVm(int Id, string Seamstress, int Planned, int Produced, decimal Price, decimal Total, string Status);

public sealed class DistribuicaoCreateVm
{
    public int ProductionId { get; set; }
    public int ProductionProcessId { get; set; }
    public string ProductionDescription { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int SeamstressId { get; set; }
    [Range(1, int.MaxValue)] public int PlannedQuantity { get; set; }
    public decimal PricePerPiece { get; set; }
    public IReadOnlyList<LookupVm> Seamstresses { get; set; } = [];
    public int PieceVariantId { get; set; }
}

public sealed class ProgressoVm
{
    public int AssignmentId { get; set; }
    [Range(0, int.MaxValue)] public int ProducedQuantity { get; set; }
}