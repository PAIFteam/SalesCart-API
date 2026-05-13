using SalesCart.Core.Application.UseCases.SalesCartUser;
using SalesCart.Core.Domain.Entities;


namespace SalesCart.API.Extensions
{
    public static class SalesCartEndpointsExtensions
    {
        public static void MapSalesCartEndpoints(this WebApplication app)
        {

            var api = app.MapGroup("/salescart/api");

            api.MapGet("/get/{idUser:int}", async (
                int idUser,
                SalesCartUserUseCase salesCartUserUseCase) =>
            {
                var salesCart = await salesCartUserUseCase.GetSalesCartUserAsync(idUser);
                return salesCart is null ? Results.NotFound() : Results.Ok(salesCart);
            })
                .WithName("GetSalesCartIdUser")
                .WithSummary("Buscar /Carrinho de Venda")
                .Produces<SalesCartUser>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization(policy => policy.RequireRole("Admin","User"));


            api.MapPut("/put", async (
                SalesCartUserInput salesCartUser,
                SalesCartUserUseCase salesCartUserUseCase,
                ILogger<Program> logger) =>
                    {
                        try
                        {
                            var result = await salesCartUserUseCase.PutSalesCartUserAsync(salesCartUser);

                            if (result == null)
                                return Results.NotFound("Nenhum processo executado com os critérios fornecidos.");

                            return Results.Ok(result);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Um erro ocorreu ao processar a inclusão do Carrinho de Vendas.");
                            return Results.BadRequest("Um erro ocorreu ao processar sua solicitação.");
                        }

                    })
                .WithName("PutSalesCartUserAsync")
                .WithDescription("Grava Carrinho de Compras do Usuário/Cliente")
                .Produces<SalesCartUserOutput>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization(policy => policy.RequireRole("User", "Admin"));

            api.MapPost("/delete", async (
                int idUser,
                SalesCartUserUseCase salesCartUserUseCase,
                ILogger<Program> logger) =>
            {
                try
                {
                    var result = await salesCartUserUseCase.ClearSalesCartUserAsync(idUser);

                    if (result == null)
                        return Results.NotFound("Nenhum processo executado com os critérios fornecidos.");

                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Um erro ocorreu ao processar a limpeza do Carrinho de Vendas.");
                    return Results.BadRequest("Um erro ocorreu ao processar sua solicitação.");
                }

            })
                .WithName("ClearSalesCartUserAsync")
                .WithDescription("Limpa o Carrinho de Compras do Usuário/Cliente")
                .Produces<SalesCartUserOutput>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization(policy => policy.RequireRole("User", "Admin"));
        }  
    }
}