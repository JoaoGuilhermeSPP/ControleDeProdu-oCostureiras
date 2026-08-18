namespace CosturaProducao.Models;



public enum ProductionStatus
{
    Pending,
    InProgress,
    Completed
}

public enum AssignmentStatus
{
    Pending,
    InProgress,
    Partial,
    Completed
}

public sealed class Production : AuditableEntity
{
    public int ClientId { get; set; }

    public Client Client { get; set; } = null!;

    public int PieceModelId { get; set; }

    public PieceModel PieceModel { get; set; } = null!;

    public int PieceVariantId { get; set; }

    public PieceVariant PieceVariant { get; set; } = null!;

    public int TotalQuantity { get; set; }

    public DateTime ProductionDate { get; set; }

    public DateTime DeliveryDate { get; set; }

    public string? Notes { get; set; }

    public ProductionStatus Status { get; set; }
        = ProductionStatus.Pending;

    public ICollection<ProductionProcess> Processes { get; set; }
        = new List<ProductionProcess>();
}
public sealed class ProductionProcess : AuditableEntity
{
    public int ProductionId { get; set; }
    public Production Production { get; set; } = null!;
    public int ServiceProcessId { get; set; }
    public ServiceProcess ServiceProcess { get; set; } = null!;
    public decimal PricePerPiece { get; set; }
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}

public sealed class Assignment : AuditableEntity
{
    public int ProductionProcessId { get; set; }
    public ProductionProcess ProductionProcess { get; set; } = null!;
    public int SeamstressId { get; set; }
    public Seamstress Seamstress { get; set; } = null!;
    public int PlannedQuantity { get; set; }
    public int ProducedQuantity { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
    public decimal PricePerPiece { get; set; }
    public decimal TotalAmount => PlannedQuantity * PricePerPiece;
}