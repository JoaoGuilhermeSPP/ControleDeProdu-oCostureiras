namespace CosturaProducao.ViewModels;

using Microsoft.AspNetCore.Http;

public sealed class CadastroInputVm
{
    public string Tipo { get; set; } = "costureiras";

    public int? Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Codigo { get; set; }

    public string? Telefone { get; set; }

    public string? Endereco { get; set; }

    public string? Descricao { get; set; }

    public string? Observacoes { get; set; }

    public string? CaminhoGabarito { get; set; }

    public IFormFile? Gabarito { get; set; }

    public decimal? ValorPorPeca { get; set; }

    public string? GabaritoAtual { get; set; }

    // Variantes da peça
    public List<PieceVariantInputVm> Variants { get; set; }
        = new();
}


public sealed class PieceVariantInputVm
{
    public int? Id { get; set; }

    public string Cor { get; set; } = string.Empty;

    public string Tamanho { get; set; } = string.Empty;
}


public sealed record CadastroRowVm(
    int Id,
    string Nome,
    string? Codigo,
    string? Complemento,
    bool Ativo
);


public sealed class CadastroIndexVm
{
    public string Tipo { get; init; } = string.Empty;

    public string Titulo { get; init; } = string.Empty;

    public IReadOnlyList<CadastroRowVm> Itens { get; init; } = [];
}