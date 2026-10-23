using DoaFacil.Api.Contracts;
using DoaFacil.Api.Data;
using DoaFacil.Api.Infrastructure;
using DoaFacil.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoaFacil.Api.Endpoints;

public static class DoacoesEndpoints
{
    public static IEndpointRouteBuilder MapDoacoesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/doacoes")
                       .WithTags("Doações");

        group.MapGet("/", ListarAsync)
             .WithName("ListarDoacoes")
             .WithSummary("Lista as doações")
             .WithDescription("Filtros opcionais: status, categoria e pontoColetaId.");

        group.MapGet("/{id:int}", ObterPorIdAsync)
             .WithName("ObterDoacao")
             .WithSummary("Busca uma doação pelo id");

        group.MapPost("/", CriarAsync)
             .WithValidation<DoacaoRequest>()
             .WithName("RegistrarDoacao")
             .WithSummary("Registra uma nova doação em um ponto de coleta ativo");

        group.MapPut("/{id:int}", AtualizarAsync)
             .WithValidation<DoacaoRequest>()
             .WithName("AtualizarDoacao")
             .WithSummary("Atualiza os dados de uma doação");

        group.MapPatch("/{id:int}/status", AtualizarStatusAsync)
             .WithValidation<AtualizarStatusDoacaoRequest>()
             .WithName("AtualizarStatusDoacao")
             .WithSummary("Altera o status de uma doação");

        group.MapDelete("/{id:int}", RemoverAsync)
             .WithName("RemoverDoacao")
             .WithSummary("Remove uma doação");

        return app;
    }

    private static async Task<Ok<List<DoacaoResponse>>> ListarAsync(
        StatusDoacao? status,
        CategoriaDoacao? categoria,
        int? pontoColetaId,
        DoaFacilDbContext db,
        CancellationToken cancellationToken)
    {
        var query = db.Doacoes.AsNoTracking().Include(d => d.PontoColeta).AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        if (categoria.HasValue)
        {
            query = query.Where(d => d.Categoria == categoria.Value);
        }

        if (pontoColetaId.HasValue)
        {
            query = query.Where(d => d.PontoColetaId == pontoColetaId.Value);
        }

        var doacoes = await query
            .OrderByDescending(d => d.RecebidaEm)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(doacoes.Select(DoacaoResponse.FromEntity).ToList());
    }

    private static async Task<Results<Ok<DoacaoResponse>, NotFound<ProblemDetails>>> ObterPorIdAsync(
        int id,
        DoaFacilDbContext db,
        CancellationToken cancellationToken)
    {
        var doacao = await db.Doacoes
            .AsNoTracking()
            .Include(d => d.PontoColeta)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        return doacao is null
            ? TypedResults.NotFound(ApiProblems.DoacaoNaoEncontrada(id))
            : TypedResults.Ok(DoacaoResponse.FromEntity(doacao));
    }

    private static async Task<Results<Created<DoacaoResponse>, ValidationProblem>> CriarAsync(
        DoacaoRequest request,
        DoaFacilDbContext db,
        CancellationToken cancellationToken)
    {
        var ponto = await db.PontosColeta.FindAsync([request.PontoColetaId], cancellationToken);
        var erroPonto = ValidarPontoColeta(ponto, request.PontoColetaId);
        if (erroPonto is not null)
        {
            return erroPonto;
        }

        var doacao = new Doacao
        {
            NomeDoador = request.NomeDoador.Trim(),
            Descricao = request.Descricao.Trim(),
            Categoria = request.Categoria,
            Quantidade = request.Quantidade,
            Status = StatusDoacao.Recebida,
            RecebidaEm = DateTime.UtcNow,
            PontoColetaId = request.PontoColetaId,
            PontoColeta = ponto
        };

        db.Doacoes.Add(doacao);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/doacoes/{doacao.Id}", DoacaoResponse.FromEntity(doacao));
    }

    private static async Task<Results<Ok<DoacaoResponse>, NotFound<ProblemDetails>, ValidationProblem>> AtualizarAsync(
        int id,
        DoacaoRequest request,
        DoaFacilDbContext db,
        CancellationToken cancellationToken)
    {
        var doacao = await db.Doacoes
            .Include(d => d.PontoColeta)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (doacao is null)
        {
            return TypedResults.NotFound(ApiProblems.DoacaoNaoEncontrada(id));
        }

        if (doacao.PontoColetaId != request.PontoColetaId)
        {
            var novoPonto = await db.PontosColeta.FindAsync([request.PontoColetaId], cancellationToken);
            var erroPonto = ValidarPontoColeta(novoPonto, request.PontoColetaId);
            if (erroPonto is not null)
            {
                return erroPonto;
            }

            doacao.PontoColetaId = request.PontoColetaId;
            doacao.PontoColeta = novoPonto;
        }

        doacao.NomeDoador = request.NomeDoador.Trim();
        doacao.Descricao = request.Descricao.Trim();
        doacao.Categoria = request.Categoria;
        doacao.Quantidade = request.Quantidade;
        doacao.AtualizadaEm = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(DoacaoResponse.FromEntity(doacao));
    }

    private static async Task<Results<Ok<DoacaoResponse>, NotFound<ProblemDetails>, Conflict<ProblemDetails>>> AtualizarStatusAsync(
        int id,
        AtualizarStatusDoacaoRequest request,
        DoaFacilDbContext db,
        CancellationToken cancellationToken)
    {
        var doacao = await db.Doacoes
            .Include(d => d.PontoColeta)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (doacao is null)
        {
            return TypedResults.NotFound(ApiProblems.DoacaoNaoEncontrada(id));
        }

        if (!TransicaoPermitida(doacao.Status, request.Status))
        {
            return TypedResults.Conflict(ApiProblems.Conflito(
                "Transição de status inválida",
                $"Não é possível alterar o status de '{doacao.Status}' para '{request.Status}'."));
        }

        doacao.Status = request.Status;
        doacao.AtualizadaEm = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(DoacaoResponse.FromEntity(doacao));
    }

    private static async Task<Results<NoContent, NotFound<ProblemDetails>>> RemoverAsync(
        int id,
        DoaFacilDbContext db,
        CancellationToken cancellationToken)
    {
        var doacao = await db.Doacoes.FindAsync([id], cancellationToken);
        if (doacao is null)
        {
            return TypedResults.NotFound(ApiProblems.DoacaoNaoEncontrada(id));
        }

        db.Doacoes.Remove(doacao);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    private static ValidationProblem? ValidarPontoColeta(PontoColeta? ponto, int pontoColetaId)
    {
        string? erro = ponto switch
        {
            null => $"Não existe ponto de coleta com id {pontoColetaId}.",
            { Ativo: false } => $"O ponto de coleta '{ponto.Nome}' está inativo e não pode receber doações.",
            _ => null
        };

        return erro is null
            ? null
            : TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { [nameof(DoacaoRequest.PontoColetaId)] = [erro] },
                title: "Ponto de coleta inválido.");
    }

    private static bool TransicaoPermitida(StatusDoacao atual, StatusDoacao novo) => (atual, novo) switch
    {
        (StatusDoacao.Recebida, StatusDoacao.EmTriagem) => true,
        (StatusDoacao.EmTriagem, StatusDoacao.Distribuida) => true,
        (StatusDoacao.Recebida or StatusDoacao.EmTriagem, StatusDoacao.Cancelada) => true,
        _ => false
    };
}
