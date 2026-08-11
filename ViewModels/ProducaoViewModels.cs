using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CosturaProducao.ViewModels;

public sealed class ProducaoCreateVm
{
    [Range(1, int.MaxValue)] public int ClientId { get; set; }
    [Range(1, int.MaxValue)] public int PieceModelId { get; set; }
    [Required, StringLength(120)] public string Color { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int TotalQuantity { get; set; }
    [BindRequired, DataType(DataType.Date)] public DateTime ProductionDate { get; set; } = DateTime.Today;
    [BindRequired, DataType(DataType.Date)] public DateTime DeliveryDate { get; set; } = DateTime.Today.AddDays(7);
    public string? Notes { get; set; }
    public List<int> ServiceProcessIds { get; set; } = [];
    public IReadOnlyList<LookupVm> Clients { get; init; } = [];
    public IReadOnlyList<LookupVm> Pieces { get; init; } = [];
    public IReadOnlyList<ServiceLookupVm> Services { get; init; } = [];
}

public sealed record LookupVm(int Id, string Name);
public sealed record ServiceLookupVm(int Id, string Name, decimal Price);

public sealed record ProducaoRowVm(int Id, string Client, string Piece, string Color, int Quantity, DateTime DeliveryDate, string Status);

public sealed class ProducaoIndexVm
{
    public IReadOnlyList<ProducaoRowVm> Items { get; init; } = [];
}