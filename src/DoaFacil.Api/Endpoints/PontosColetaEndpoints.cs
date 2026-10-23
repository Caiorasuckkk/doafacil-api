using DoaFacil.Api.Contracts;
using DoaFacil.Api.Data;
using DoaFacil.Api.Infrastructure;
using DoaFacil.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoaFacil.Api.Endpoints;

public static class PontosColetaEndpoints
{
    public static IEndpointRouteBuilder MapPontosColetaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pontos-coleta")
                       .WithTags("Pontos de Coleta");

        group.MapGet("/", ListarAsync)
             .WithName("ListarPontosColeta")
             .WithSummary("Lista os pontos de coleta")
             .WithDescription("Filtros opcionais: cidade e ativo.");

        group.MapGet("/{id:int}", ObterPorIdAsync)
             .WithName("ObterPontoColeta")
             .WithSummary("Busca um ponto de coleta pelo id");

        group.MapGet("/{id:int}/doacoes", ListarDoacoesDoPontoAsync)
             .WithName("ListarDoacoesDoPontoColeta")
             .WithSummary("Lista as doações recebidas por um ponto de coleta");

        group.MapPost("/", CriarAsync)
             .WithValidation<PontoColetaRequest>()
             .WithName("CriarPontoColeta")
             .WithSummary("Cadastra um novo ponto de coleta");

        group.MapPut("/{id:int}", AtualizarAsync)
             .WithValidation<PontoColetaRequest>()
             .WithName("AtualizarPontoColeta")
             .WithSummary("Atualiza todos os dados de um ponto de coleta");

        group.MapDelete("/{id:int}", RemoverAsync)
             .WithName("RemoverPontoColeta")
             .WithSummary("Remove um ponto de coleta sem doações vinculadas");

        return app;
    }

    private static async Task<Ok<List<PontoColetaResponse>>> ListarAsync(
        string? cidade,
        bool? ativo,
        DoaFacilDbContext db,
        CancellationToken cancellationToken)
    {
        var query = db.PontosColeta.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(cidade))
        {
            var filtro = cidade.Trim().ToLower();
            query = query.Where(p => p.Cidade.ToLower().Contains(filtro));
        }

        if (ativo.HasValue)
        {
            query = query.Where(p => p.Ativo == ativo.Value);
        }

        var pontos = await query
            .OrderBy(p => p.Nome)
            .Select(p => new PontoColetaResponse(
                p.Id, p.Nome, p.Endereco, p.Cidade, p.Uf, p.Telefone, p.Ativo, p.CriadoEm, p.Doacoes.Count))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(pontos);
    }

    private static async Task<Results<Ok<PontoColetaResponse>, NotFound<ProblemDetails>>> ObterPorIdAsync(
        int id,
        DoaFacilDbContext db,
        CancellationToken cancellationToken)
    {
        var ponto = await db.PontosColeta
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PontoColetaResponse(
                p.Id, p.Nome, p.Endereco, p.Cidade, p.Uf, p.Telefone, p.Ativo, p.CriadoEm, p.Doacoes.Count))
            .FirstOrDefaultAsync(cancellationToken);

        return ponto is null
            ? TypedResults.NotFound(ApiProblems.PontoColetaNaoEncontrado(id))
            : TypedResults.Ok(ponto);
    }

    private static async Task<Results<Ok<List<DoacaoResponse>>, NotFound<ProblemDetails>>> ListarDoacoesDoPontoAsync(
        int id,
        DoaFacilDbContext db,
        CancellationToken cancellationToken)
    {
        var pontoExiste = await db.PontosColeta.AnyAsync(p => p.Id == id, cancellationToken);
        if (!pontoExiste)
        {
            return TypedResults.NotFound(ApiProblems.PontoColetaNaoEncontrado(id));
        }

        var doacoes = await db.Doacoes
            .AsNoTracking()
            .Include(d => d.PontoColeta)
            .Where(d => d.PontoColetaId == id)
            .OrderByDescending(d => d.RecebidaEm)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(doacoes.Select(DoacaoResponse.FromEntity).ToList());
    }

    private static async Task<Results<Created<PontoColetaResponse>, Conflict<ProblemDetails>>> CriarAsync(
        PontoColetaRequest request,
        DoaFacilDbContext db,
        CancellationToken cancellationToken)
    {
        var nome = request.Nome.Trim();
        var cidade = request.Cidade.Trim();

        if (await ExisteDuplicadoAsync(db, nome, cidade, idIgnorado: null, cancellationToken))
        {
            return TypedResults.Conflict(ApiProblems.PontoColetaDuplicado(nome, cidade));
        }

        var ponto = new PontoColeta
        {
            Nome = nome,
            Endereco = request.Endereco.Trim(),
            Cidade = cidade,
            Uf = request.Uf.ToUpperInvariant(),
            Telefone = request.Telefone?.Trim(),
            Ativo = request.Ativo,
            CriadoEm = DateTime.UtcNow
        };

        db.PontosColeta.Add(ponto);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/pontos-coleta/{ponto.Id}", PontoColetaResponse.FromEntity(ponto, totalDoacoes: 0));
    }

    private static async Task<Results<Ok<PontoColetaResponse>, NotFound<ProblemDetails>, Conflict<ProblemDetails>>> AtualizarAsync(
        int id,
        PontoColetaRequest request,
        DoaFacilDbContext db,
        CancellationToken cancellationToken)
    {
        var ponto = await db.PontosColeta.FindAsync([id], cancellationToken);
        if (ponto is null)
        {
            return TypedResults.NotFound(ApiProblems.PontoColetaNaoEncontrado(id));
        }

        var nome = request.Nome.Trim();
        var cidade = request.Cidade.Trim();

        if (await ExisteDuplicadoAsync(db, nome, cidade, idIgnorado: id, cancellationToken))
        {
            return TypedResults.Conflict(ApiProblems.PontoColetaDuplicado(nome, cidade));
        }

        ponto.Nome = nome;
        ponto.Endereco = request.Endereco.Trim();
        ponto.Cidade = cidade;
        ponto.Uf = request.Uf.ToUpperInvariant();
        ponto.Telefone = request.Telefone?.Trim();
        ponto.Ativo = request.Ativo;

        await db.SaveChangesAsync(cancellationToken);

        var totalDoacoes = await db.Doacoes.CountAsync(d => d.PontoColetaId == id, cancellationToken);
        return TypedResults.Ok(PontoColetaResponse.FromEntity(ponto, totalDoacoes));
    }

    private static async Task<Results<NoContent, NotFound<ProblemDetails>, Conflict<ProblemDetails>>> RemoverAsync(
        int id,
        DoaFacilDbContext db,
        CancellationToken cancellationToken)
    {
        var ponto = await db.PontosColeta.FindAsync([id], cancellationToken);
        if (ponto is null)
        {
            return TypedResults.NotFound(ApiProblems.PontoColetaNaoEncontrado(id));
        }

        var possuiDoacoes = await db.Doacoes.AnyAsync(d => d.PontoColetaId == id, cancellationToken);
        if (possuiDoacoes)
        {
            return TypedResults.Conflict(ApiProblems.Conflito(
                "Ponto de coleta possui doações",
                $"O ponto de coleta {id} possui doações vinculadas. Desative-o (ativo = false) em vez de removê-lo."));
        }

        db.PontosColeta.Remove(ponto);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    private static Task<bool> ExisteDuplicadoAsync(
        DoaFacilDbContext db, string nome, string cidade, int? idIgnorado, CancellationToken cancellationToken)
    {
        var nomeNormalizado = nome.ToLower();
        var cidadeNormalizada = cidade.ToLower();

        return db.PontosColeta.AnyAsync(
            p => p.Nome.ToLower() == nomeNormalizado
              && p.Cidade.ToLower() == cidadeNormalizada
              && (idIgnorado == null || p.Id != idIgnorado),
            cancellationToken);
    }
}
