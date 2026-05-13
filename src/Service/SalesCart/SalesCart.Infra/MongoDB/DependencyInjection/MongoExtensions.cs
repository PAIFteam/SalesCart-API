using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SalesCart.Infra.MongoDB.Contexts;

namespace SalesCart.Infra.MongoDB.DependencyInjection;

public static class MongoExtensions
{
    public static IServiceCollection AddMongoDB(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionStrings = configuration.GetConnectionString("PaifGamesMongoDb");
        services.AddSingleton<IMongoClient>(_ =>
                new MongoClient(MongoUrl.Create(connectionStrings)))
            .AddSingleton<IReferenceArchitectureDatabase,ReferenceArchitectureDatabase>();

        return services;
    }
}
