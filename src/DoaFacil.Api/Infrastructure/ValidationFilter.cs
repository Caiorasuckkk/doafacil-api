using System.ComponentModel.DataAnnotations;

namespace DoaFacil.Api.Infrastructure;

public sealed class ValidationFilter<TRequest> : IEndpointFilter where TRequest : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
        {
            return TypedResults.Problem(
                title: "Requisição inválida",
                detail: "O corpo da requisição é obrigatório.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        if (isValid)
        {
            return await next(context);
        }

        var errors = results
            .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty), (r, member) => new { member, r.ErrorMessage })
            .GroupBy(e => e.member)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage ?? "Valor inválido.").ToArray());

        return TypedResults.ValidationProblem(errors, title: "Um ou mais campos são inválidos.");
    }
}

public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder) where TRequest : class
        => builder.AddEndpointFilter<ValidationFilter<TRequest>>()
                  .ProducesValidationProblem();
}
