using Freelando.Api.Converters;
using Freelando.Api.Requests;
using Freelando.Dados;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using System;
using Freelando.Dados.UnitOfWork;
using Microsoft.Extensions.Caching.Memory;
using Freelando.Api.Responses;
using Freelando.Api.Services;


namespace Freelando.Api.Endpoints;

public static class ClienteExtension
{
    public static void AddEndPointClientes(this WebApplication app)
    {
        const string chaveCache = "clientes";

        app.MapGet("/clientes", async ([FromServices] ClienteConverter converter, [FromServices] IUnitOfWork unitOfWork,IMemoryCache cache) =>
        {           

            if(!cache.TryGetValue(chaveCache,out ICollection<ClienteResponse> clientesCache))
            {
                clientesCache = converter.EntityListToResponseList(await unitOfWork.ClienteRepository.BuscarTodos());
                cache.Set(chaveCache, clientesCache,new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }                                   

            return Results.Ok(await Task.FromResult(clientesCache));

        }).WithTags("Cliente").WithOpenApi();

        app.MapGet("/clientes/redis", async ([FromServices] ClienteConverter converter, [FromServices] IUnitOfWork unitOfWork, [FromServices] ICacheService cacheService) =>
        {
            var clientesCache = await cacheService.GetCachedDataAsync<IEnumerable<ClienteResponse>>(chaveCache);

            if (clientesCache != null)
            {
                return Results.Ok(clientesCache);
            }
            var clientes = converter.EntityListToResponseList(await unitOfWork.ClienteRepository.BuscarTodos());
            await cacheService.SetCachedDataAsync(chaveCache, clientes, TimeSpan.FromMinutes(5));

            return Results.Ok(await Task.FromResult(clientes));

        }).WithTags("Cliente").WithOpenApi();


        app.MapGet("/clientes/identificador-nome", async ([FromServices] ClienteConverter converter, [FromServices] IUnitOfWork unitOfWork) =>
        {
            var clientes = unitOfWork.contexto.Clientes.Select(c => new {Identificador=c.Id,Nome=c.Nome });
            return Results.Ok(await Task.FromResult(clientes));
        }).WithTags("Cliente").WithOpenApi();

        app.MapGet("/clientes/projeto-especialidade", async ([FromServices] ClienteConverter converter, [FromServices] IUnitOfWork unitOfWork) =>
        {
            var clientes = unitOfWork.contexto.Clientes.Include(x => x.Projetos).ThenInclude(p => p.Especialidades).AsSplitQuery().ToList();

            return Results.Ok(await Task.FromResult(clientes));
        }).WithTags("Cliente").WithOpenApi();

        app.MapGet("/clientes/por-email", async ([FromServices] ClienteConverter converter, [FromServices] IUnitOfWork unitOfWork, string email) =>
        {
            var clientes = unitOfWork.contexto.Clientes.Where(c => c.Email.Equals(email)).ToList();

            return Results.Ok(await Task.FromResult(clientes));
        }).WithTags("Cliente").WithOpenApi();

        app.MapPost("/cliente", async ([FromServices] ClienteConverter converter, [FromServices] IUnitOfWork unitOfWork, ClienteRequest clienteRequest) =>
        {
            var cliente = converter.RequestToEntity(clienteRequest);    
            await unitOfWork.ClienteRepository.Adicionar(cliente);      
            await unitOfWork.Commit();

            return Results.Created($"/cliente/{cliente.Id}", cliente);
        }).WithTags("Cliente").WithOpenApi();

        app.MapPut("/cliente/{id}", async ([FromServices] ClienteConverter converter, [FromServices] IUnitOfWork unitOfWork, Guid id, ClienteRequest clienteRequest) =>
        {
            var cliente = await unitOfWork.ClienteRepository.BuscarPorId(x => x.Id == id);
            if (cliente is null)
            {
                return Results.NotFound();
            }
            var clienteAtualizado = converter.RequestToEntity(clienteRequest);
            cliente.Nome = clienteAtualizado.Nome;
            cliente.Cpf = clienteAtualizado.Cpf;
            cliente.Email = clienteAtualizado.Email;
            cliente.Telefone = clienteAtualizado.Telefone;

            await unitOfWork.ClienteRepository.Atualizar(cliente);
            await unitOfWork.Commit();

            return Results.Ok((cliente));
        }).WithTags("Cliente").WithOpenApi();

        app.MapDelete("/cliente/{id}", async ([FromServices] ClienteConverter converter, [FromServices] IUnitOfWork unitOfWork, Guid id) =>
        {
            var cliente = await unitOfWork.ClienteRepository.BuscarPorId(x => x.Id == id);
            if (cliente is null)
            {
                return Results.NotFound();
            }

            await unitOfWork.ClienteRepository.Deletar(cliente);
            await unitOfWork.Commit();

            return Results.NoContent();
        }).WithTags("Cliente").WithOpenApi();


        app.MapGet("/clientes/parametro", async ([FromServices] ClienteConverter converter, [FromServices] IUnitOfWork unitOfWork, string parametro) =>
        {
         
            var clientes = unitOfWork.contexto.Clientes.FromSqlRaw("SELECT * FROM TB_Clientes WHERE Nome ={0}",parametro).ToList();

            return Results.Ok(await Task.FromResult(clientes));
        }).WithTags("Cliente").WithOpenApi();

    }
}
