using MediatR;

namespace Catalog.API.Products.UpdateProduct; 

public record UpdateProductRequest(Guid Id, string Name, List<string> Category ,string Description, decimal Price);
public record UpdateProductResponse(bool IsSuccess);
public class UpdateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products", async (ISender sender, UpdateProductRequest request, CancellationToken cancellationToken) =>
        {
            var command = request.Adapt<UpdateProductCommand>();
            var result = await sender.Send(command, cancellationToken);
            var response = new UpdateProductResponse(true);
            return Results.Ok(response);
        })
        .WithName("UpdateProduct")
        .Produces<UpdateProductResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Update Product")
        .WithDescription("Update Product");
    }
}
