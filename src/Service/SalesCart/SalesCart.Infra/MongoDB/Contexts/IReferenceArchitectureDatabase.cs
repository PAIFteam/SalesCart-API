using MongoDB.Driver;

namespace SalesCart.Infra.MongoDB.Contexts
{
    public interface IReferenceArchitectureDatabase
    {
        IMongoDatabase Database { get; }
    }
}
