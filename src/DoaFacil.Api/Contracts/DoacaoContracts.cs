using System.ComponentModel.DataAnnotations;
using DoaFacil.Api.Models;

namespace DoaFacil.Api.Contracts;

public sealed class DoacaoRequest
{
    [Required(ErrorMessage = "O nome do doador é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome do doador deve ter entre 2 e 100 caracteres.")]
    public string NomeDoador { get; init; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(300, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 300 caracteres.")]
    public string Descricao { get; init; } = string.Empty;

    [EnumDataType(typeof(CategoriaDoacao), ErrorMessage = "Categoria inválida.")]
    public CategoriaDoacao Categoria { get; init; }

    [Range(1, 10_000, ErrorMessage = "A quantidade deve estar entre 1 e 10000.")]
    public int Quantidade { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe um ponto de coleta válido.")]
    public int PontoColetaId { get; init; }
}

public sealed class AtualizarStatusDoacaoRequest
{
    [EnumDataType(typeof(StatusDoacao), ErrorMessage = "Status inválido.")]
    public StatusDoacao Status { get; init; }
}

public sealed record DoacaoResponse(
    int Id,
    string NomeDoador,
    string Descricao,
    CategoriaDoacao Categoria,
    int Quantidade,
    StatusDoacao Status,
    DateTime RecebidaEm,
    DateTime? AtualizadaEm,
    int PontoColetaId,
    string? NomePontoColeta)
{
    public static DoacaoResponse FromEntity(Doacao doacao) => new(
        doacao.Id,
        doacao.NomeDoador,
        doacao.Descricao,
        doacao.Categoria,
        doacao.Quantidade,
        doacao.Status,
        doacao.RecebidaEm,
        doacao.AtualizadaEm,
        doacao.PontoColetaId,
        doacao.PontoColeta?.Nome);
}
