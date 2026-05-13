
using MediatR;

namespace Catalog.API.Products.GetProducts;

//public record GetProductsRequest();
public record GetProductsResponse(IEnumerable<ProductModel> Products);
public class GetProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetProductsQuery(), cancellationToken);
            var response =  result.Adapt<GetProductsResponse>();    
            return Results.Ok(response);
        })
        .WithName("GetProducts")
        .Produces<GetProductsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Products")
        .WithDescription("Get Products");
    }
}
