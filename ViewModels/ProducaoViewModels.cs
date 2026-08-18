namespace CosturaProducao.ViewModels;

public sealed class ProducaoCreateVm
{
    public int ClientId { get; set; }

    public int PieceModelId { get; set; }

    public int PieceVariantId { get; set; }

    public int TotalQuantity { get; set; }

    public DateTime ProductionDate { get; set; }

    public DateTime DeliveryDate { get; set; }

    public string? Notes { get; set; }

    public List<int> ServiceProcessIds { get; set; } = new();


    public IReadOnlyList<LookupVm> Clients { get; set; }
        = Array.Empty<LookupVm>();

    public IReadOnlyList<LookupVm> Pieces { get; set; }
        = Array.Empty<LookupVm>();

    public IReadOnlyList<PieceVariantLookupVm> Variants { get; set; }
        = Array.Empty<PieceVariantLookupVm>();

    public IReadOnlyList<ServiceLookupVm> Services { get; set; }
        = Array.Empty<ServiceLookupVm>();
}


public sealed record LookupVm(
    int Id,
    string Name
);


public sealed record ServiceLookupVm(
    int Id,
    string Name,
    decimal Price
);


public sealed record PieceVariantLookupVm(
    int Id,
    string Cor,
    string Tamanho
);


public sealed record ProducaoRowVm(
    int Id,
    string Cliente,
    string Peca,
    string Cor,
    string Tamanho,
    int Quantidade,
    DateTime DataEntrega,
    string Status
);


public sealed class ProducaoIndexVm
{
    public IReadOnlyList<ProducaoRowVm> Items { get; init; } = [];
}