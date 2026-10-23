using Microsoft.AspNetCore.Mvc;

namespace DoaFacil.Api.Infrastructure;

public static class ApiProblems
{
    public static ProblemDetails PontoColetaNaoEncontrado(int id) =>
        NaoEncontrado("Ponto de coleta não encontrado", $"Não existe ponto de coleta com id {id}.");

    public static ProblemDetails DoacaoNaoEncontrada(int id) =>
        NaoEncontrado("Doação não encontrada", $"Não existe doação com id {id}.");

    public static ProblemDetails PontoColetaDuplicado(string nome, string cidade) =>
        Conflito("Ponto de coleta duplicado", $"Já existe um ponto de coleta chamado '{nome}' em {cidade}.");

    public static ProblemDetails NaoEncontrado(string title, string detail) => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = title,
        Detail = detail
    };

    public static ProblemDetails Conflito(string title, string detail) => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = title,
        Detail = detail
    };
}
