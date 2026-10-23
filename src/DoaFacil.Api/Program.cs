using System.Text.Json.Serialization;
using DoaFacil.Api.Data;
using DoaFacil.Api.Endpoints;
using DoaFacil.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DoaFacilDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DoaFacil")));

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "DoaFácil API";
        document.Info.Version = "v1";
        document.Info.Description = "API para gestão de pontos de coleta e doações para ONGs e iniciativas comunitárias.";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("DoaFácil API"));

    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<DoaFacilDbContext>();
    await db.Database.MigrateAsync();
}

app.MapGet("/", () => TypedResults.Redirect("/scalar")).ExcludeFromDescription();

app.MapPontosColetaEndpoints();
app.MapDoacoesEndpoints();

await app.RunAsync();
