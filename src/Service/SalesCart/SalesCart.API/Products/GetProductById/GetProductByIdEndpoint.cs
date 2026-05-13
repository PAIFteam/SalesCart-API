using MediatR;

namespace Catalog.API.Products.GetProductById;

//public record GetUserByIdRequest(Guid Id);
public record GetProductByIdResponse(ProductModel Product);
public class GetProductByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:guid}", async (ISender sender, Guid id,  CancellationToken cancellationToken) =>
        {
            var result = await  sender.Send( new GetUserByIdQuery(id), cancellationToken);
            var response = result.Adapt<GetProductByIdResponse>();
            return Results.Ok(response);
        })
        .WithName("GetProductById")
        .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get Product By Id")
        .WithDescription("Get Product By Id");
    }
}
