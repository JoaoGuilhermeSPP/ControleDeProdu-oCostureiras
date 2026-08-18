namespace CosturaProducao.Models;


public sealed class PieceVariant : AuditableEntity
{
    public int PieceModelId { get; set; }

    public PieceModel PieceModel { get; set; } = null!;

    public string Cor { get; set; } = string.Empty;

    public string Tamanho { get; set; } = string.Empty;

    public ICollection<Production> Productions { get; set; }
        = new List<Production>();
}