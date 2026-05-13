using System;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;

namespace SalesCart.Infra.MongoDB.Contexts;
internal sealed class ReferenceArchitectureDatabase : IReferenceArchitectureDatabase
{
    public ReferenceArchitectureDatabase(IMongoClient client)
    {
        Database = client.GetDatabase("PaifGamesMongoDb");

        var conventionPack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true)
        };

        ConventionRegistry.Register("camelCase", conventionPack, t => true);

        try
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }
        catch (BsonSerializationException ex)
        {
            Console.WriteLine($"Error registering conventions: {ex.Message}");
        }
    }

    public IMongoDatabase Database { get; }
}
