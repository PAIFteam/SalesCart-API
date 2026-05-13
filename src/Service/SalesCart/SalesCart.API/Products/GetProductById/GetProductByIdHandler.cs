namespace Catalog.API.Products.GetProductById;

public record GetUserByIdQuery(Guid Id) : IQuery<GetProductByIdResult>;
public record GetProductByIdResult(ProductModel Product);
internal class GetProductByIdQueryHandler(IDocumentSession session, ILogger<GetProductByIdQueryHandler> logger) 
    : IQueryHandler<GetUserByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("GetProductByIdQueryHandler.Handle called with {@Query}", query);
        var product = await session.LoadAsync<ProductModel>(query.Id, cancellationToken);

        if(product is null)
        {
            throw new ProductNotFoundException(); 
        }

        return new GetProductByIdResult(product);
    }
}
