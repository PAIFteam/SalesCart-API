namespace Catalog.API.Products;

public sealed class ProductNotFoundException : Exception
{
    public ProductNotFoundException() : base("Product not found")
    {
    }
}
