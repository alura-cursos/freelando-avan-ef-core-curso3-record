using Freelando.Dados.UnitOfWork;
using Freelando.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Freelando.Api.Endpoints;
public static class PropostasExtensions
{
    public static void AddEndPointPropostas(this WebApplication app)
    {
        app.MapGet("/propostas", async ([FromServices] IUnitOfWork unitOfOrk) =>
        {
            var propostas = await unitOfOrk.contexto.Propostas.FromSqlRaw("EXEC dbo.sp_BuscarTodasPropostas").ToListAsync();

           return Results.Ok(propostas);

        }).WithTags("Propostas").WithOpenApi();

        app.MapGet("/propostas/projeto", async ([FromServices] IUnitOfWork unitOfOrk, [FromQuery] Guid id_projeto) =>
        {
            var propostas = await unitOfOrk.contexto.Propostas.FromSqlRaw("EXEC dbo.sp_PropostaPorProjeto @id_projeto={0}", id_projeto).ToListAsync();

            return Results.Ok(propostas);

        }).WithTags("Propostas").WithOpenApi();

        app.MapGet("/propostas/summary", async ([FromServices] IUnitOfWork unitOfOrk) =>
        {
            var propostasSummmary = await unitOfOrk.contexto.Database.SqlQueryRaw<PropostaSummary>("EXEC dbo.sp_PropostaSummary").ToListAsync();

            return Results.Ok(propostasSummmary);

        }).WithTags("Propostas").WithOpenApi();

    }
}

public class PropostaSummary
{
    public Guid Id_Proposta { get; set; }
    public DateTime Data_Proposta { get; set; }
    public DateTime Prazo_Entrega { get; set; }
}


