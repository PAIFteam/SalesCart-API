using SalesCart.Core.Domain.Entities;
using SalesCart.Core.Domain.Interfaces;
using MongoDB.Driver;
using SalesCart.Infra.MongoDB.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using MongoDB.Bson.Serialization;


namespace SalesCart.Infra.Data.Repositories
{
    public class SalesCartRepository : ISalesCartUserRepository
    {
        private readonly IReferenceArchitectureDatabase _database;
        private readonly ILogger<SalesCartRepository> _logger;
        public SalesCartRepository(
            IConfiguration configuration,
            IReferenceArchitectureDatabase database,
            ILogger<SalesCartRepository> logger)
        {
            _database = database;
            _logger = logger;

            RegisterSalesCartClass<SalesCartUser>(false);
            _ = DropOldIndexAsync("IdUser_1");
            _ = CreateIndexAsyc();
        }

        private IMongoCollection<SalesCartUser> SalesCartCollection => _database.Database.GetCollection<SalesCartUser>("PaifGamesSalesCart");
        

        public async Task<SalesCartUser> GetSalesCartUserByIdUserAsync(int idUser)
        {
            try
            {
                var filter = Builders<SalesCartUser>.Filter.Where(s => s.IdUser == idUser);
                return await SalesCartCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar SalesCartUser com IdUser: {idUser}.");
                throw;
            }
        }
        public async Task PutSalesCartUserAsync(SalesCartUser salesCartUser)
        {
            try
            {
                await SalesCartCollection.ReplaceOneAsync(
                        s => s.IdUser == salesCartUser.IdUser, 
                        salesCartUser, 
                        new ReplaceOptions {IsUpsert = true}
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar SalesCartUser no MongoDB.");
                throw;
            }
        }

        public async Task DeleteSalesCartUserAsync(int idUser)
        {
            try
            {
                await SalesCartCollection.DeleteOneAsync(
                        s => s.IdUser == idUser
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir SalesCartUser no MongoDB.");
                throw;
            }
        }
        private static void RegisterSalesCartClass<T>(bool isRoot) where T : SalesCartUser
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(T))) return;

            if (!isRoot)
            {
                BsonClassMap.RegisterClassMap<T>();
                return;
            }
            
            BsonClassMap.RegisterClassMap<T>(cm =>
            {
                cm.AutoMap();
                cm.SetIsRootClass(true);
            });
        }
        private async Task DropOldIndexAsync(string indexName)
        {
            try
            { 
                await SalesCartCollection.Indexes.DropOneAsync(indexName); 
            }
            catch (Exception ex) 
            {
                _logger.LogWarning(ex, $"Não foi possível remover o índice '{indexName}'. Ele pode não existir ou já ter sido removido.");

            }
        }
        private async Task CreateIndexAsyc()
        {
            try
            { 
            var indexKeysDefinition = Builders<SalesCartUser>.IndexKeys
                .Ascending(s => s.IdUser);
            
            var indexModel = new CreateIndexModel<SalesCartUser>(indexKeysDefinition);
            await SalesCartCollection.Indexes.CreateOneAsync(indexModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar índice para IdUser.");
            }
        }
    }
}
