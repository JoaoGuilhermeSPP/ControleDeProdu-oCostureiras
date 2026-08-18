namespace CosturaProducao.Models;

public abstract class AuditableEntity
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool Active { get; set; } = true;
}

public sealed class Company : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public sealed class Seamstress : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}

public sealed class Client : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public ICollection<Production> Productions { get; set; } = new List<Production>();
}

public sealed class ServiceProcess : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultPricePerPiece { get; set; }
    public ICollection<PieceService> PieceServices { get; set; } = new List<PieceService>();
}

public sealed class PieceModel : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? TemplateImagePath { get; set; }

    public ICollection<PieceVariant> Variants { get; set; }
        = new List<PieceVariant>();

    public ICollection<PieceService> PieceServices { get; set; }
        = new List<PieceService>();

    public ICollection<Production> Productions { get; set; }
        = new List<Production>();
    public ICollection<PieceSize> PieceSizes { get; set; } = new List<PieceSize>();
}
public sealed class PieceSize
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int PieceModelId { get; set; }
    public PieceModel PieceModel { get; set; } = null!;
}

public sealed class PieceService
{

    public int Id { get; set; }
    public int PieceModelId { get; set; }
    public PieceModel PieceModel { get; set; } = null!;
    public int ServiceProcessId { get; set; }
    public ServiceProcess ServiceProcess { get; set; } = null!;
}