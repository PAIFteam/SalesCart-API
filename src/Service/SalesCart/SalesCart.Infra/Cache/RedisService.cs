using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SalesCart.Infra.Cache
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IDatabase _database;
        private readonly ILogger<RedisService> _logger;

        public RedisService(
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<RedisService> logger)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _database = _connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);

                if (value.IsNullOrEmpty)
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(value.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao recuperar valor do Redis com chave: {key}");
                throw;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var serialized = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, serialized, expiration, When.Always);
            }
            catch (Exception ex)    
            {
                _logger.LogError(ex, $"Erro ao salvar valor no Redis com chave: {key}");
                throw;
            }
        }

        public async Task DeleteAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao deletar valor do Redis com chave: {key}");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao verificar existência da chave: {key}");
                throw;
            }
        }

        public async Task FlushAsync()
        {
            try
            {
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().FirstOrDefault() 
                    ?? throw new InvalidOperationException("Redis endpoint not found"));
                await server.FlushDatabaseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar o banco de dados do Redis");
                throw;
            }
        }
    }
}
