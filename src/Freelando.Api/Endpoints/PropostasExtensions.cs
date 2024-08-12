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


    }
}


