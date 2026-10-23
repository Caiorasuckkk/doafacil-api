using System.ComponentModel.DataAnnotations;
using DoaFacil.Api.Models;

namespace DoaFacil.Api.Contracts;

public sealed class PontoColetaRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; init; } = string.Empty;

    [Required(ErrorMessage = "O endereço é obrigatório.")]
    [StringLength(200, ErrorMessage = "O endereço deve ter no máximo 200 caracteres.")]
    public string Endereco { get; init; } = string.Empty;

    [Required(ErrorMessage = "A cidade é obrigatória.")]
    [StringLength(80, ErrorMessage = "A cidade deve ter no máximo 80 caracteres.")]
    public string Cidade { get; init; } = string.Empty;

    [Required(ErrorMessage = "A UF é obrigatória.")]
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "A UF deve conter exatamente 2 letras.")]
    public string Uf { get; init; } = string.Empty;

    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
    public string? Telefone { get; init; }

    public bool Ativo { get; init; } = true;
}

public sealed record PontoColetaResponse(
    int Id,
    string Nome,
    string Endereco,
    string Cidade,
    string Uf,
    string? Telefone,
    bool Ativo,
    DateTime CriadoEm,
    int TotalDoacoes)
{
    public static PontoColetaResponse FromEntity(PontoColeta ponto, int totalDoacoes) => new(
        ponto.Id,
        ponto.Nome,
        ponto.Endereco,
        ponto.Cidade,
        ponto.Uf,
        ponto.Telefone,
        ponto.Ativo,
        ponto.CriadoEm,
        totalDoacoes);
}
