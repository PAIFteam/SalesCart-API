using SalesCart.Core.Domain.Entities;           
using SalesCart.Core.Domain.Interfaces;
using Microsoft.Extensions.Configuration;       
using Microsoft.Extensions.Logging;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesCart.Infra.Cache;

namespace SalesCart.Infra.Data.Repositories
{
    public class CuponRepository : ICuponRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CuponRepository> _logger;
        private readonly string _connectionString;
        private readonly IRedisService _redisService;
        private const string CACHE_KEY = "cupons:all";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        public CuponRepository(
            IConfiguration configuration,
            ILogger<CuponRepository> logger,
            IRedisService redisService)
        {
            _configuration = configuration;
            _logger = logger;
            _redisService = redisService;
            _connectionString = _configuration.GetConnectionString("DB_SQL_PAIF_GAMES")
                ?? throw new InvalidOperationException("Connection string 'DB_SQL_PAIF_GAMES' not found.");
        }

        public async Task<Cupon> GetCuponByIdAsync(int idCupon)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT id_cupon as IdCupon, name as CuponName, percent_desc as PercentDesc, data_start as DataStart, data_end as DataEnd, usage_limit as UsageLimit, usage_quantity as UsageQuantity, ativo as Ativo FROM [dbo].[cupon] WHERE id_cupon = @IdCupon";

                    var cupon = await connection.QueryFirstOrDefaultAsync<Cupon>(query, new { IdCupon = idCupon });
                    return cupon;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar cupom com IdCupon: {idCupon}.");
                throw;
            }
        }

        public async Task<IEnumerable<Cupon>> GetAllCuponesAsync()
        {
            try
            {
                // Tentar buscar do cache
                var cachedCupones = await _redisService.GetAsync<List<Cupon>>(CACHE_KEY);
                if (cachedCupones != null && cachedCupones.Any())
                {
                    _logger.LogInformation("Cupons recuperados do cache Redis.");
                    return cachedCupones;
                }

                // Se não estiver em cache, buscar do banco de dados
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT id_cupon as IdCupon, name as CuponName, percent_desc as PercentDesc, data_start as DataStart, data_end as DataEnd, usage_limit as UsageLimit, usage_quantity as UsageQuantity, ativo as Ativo FROM [dbo].[cupon]";

                    var cupones = (await connection.QueryAsync<Cupon>(query)).ToList();

                    // Armazenar no cache
                    if (cupones.Any())
                    {
                        await _redisService.SetAsync(CACHE_KEY, cupones, _cacheDuration);
                        _logger.LogInformation("Cupons armazenados em cache Redis.");
                    }

                    return cupones;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os cupones.");
                throw;
            }
        }

        public async Task<int> CreateCuponAsync(Cupon cupon)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"INSERT INTO [dbo].[cupon] 
                                  (id_cupon, name, percent_desc, data_start, data_end, usage_limit, usage_quantity, ativo) 
                                  VALUES 
                                  (@IdCupon, @Name, @PercentDesc, @DataStart, @DataEnd, @UsageLimit, @UsageQuantity, @Ativo);
                                  SELECT SCOPE_IDENTITY();";

                    var result = await connection.QueryFirstOrDefaultAsync<int>(query, new
                    {
                        IdCupon = cupon.IdCupon,
                        Name = cupon.CuponName ?? (object)DBNull.Value,
                        PercentDesc = cupon.PercentDesc,
                        DataStart = cupon.DataStart,
                        DataEnd = cupon.DataEnd,
                        UsageLimit = cupon.UsageLimit,
                        UsageQuantity = cupon.UsageQuantity,
                        Ativo = cupon.Ativo
                    });

                    // Invalidar cache após criação
                    await _redisService.DeleteAsync(CACHE_KEY);
                    _logger.LogInformation("Cache de cupons invalidado após criação.");

                    return result > 0 ? result : cupon.IdCupon;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cupom.");
                throw;
            }
        }

        public async Task<bool> UpdateCuponAsync(Cupon cupon)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"UPDATE [dbo].[cupon] 
                                  SET name = @CuponName, 
                                      percent_desc = @PercentDesc, 
                                      data_start = @DataStart, 
                                      data_end = @DataEnd, 
                                      usage_limit = @UsageLimit, 
                                      usage_quantity = @UsageQuantity, 
                                      ativo = @Ativo 
                                  WHERE id_cupon = @IdCupon";

                    var rowsAffected = await connection.ExecuteAsync(query, new
                    {
                        IdCupon = cupon.IdCupon,
                        Name = cupon.CuponName ?? (object)DBNull.Value,
                        PercentDesc = cupon.PercentDesc,
                        DataStart = cupon.DataStart,
                        DataEnd = cupon.DataEnd,
                        UsageLimit = cupon.UsageLimit,
                        UsageQuantity = cupon.UsageQuantity,
                        Ativo = cupon.Ativo
                    });

                    // Invalidar cache após atualização
                    if (rowsAffected > 0)
                    {
                        await _redisService.DeleteAsync(CACHE_KEY);
                        _logger.LogInformation("Cache de cupons invalidado após atualização.");
                    }

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar cupom com IdCupon: {cupon.IdCupon}.");
                throw;
            }
        }

        public async Task<bool> DeleteCuponAsync(int idCupon)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "DELETE FROM [dbo].[cupon] WHERE id_cupon = @IdCupon";

                    var rowsAffected = await connection.ExecuteAsync(query, new { IdCupon = idCupon });

                    // Invalidar cache após exclusão
                    if (rowsAffected > 0)
                    {
                        await _redisService.DeleteAsync(CACHE_KEY);
                        _logger.LogInformation("Cache de cupons invalidado após exclusão.");
                    }

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao deletar cupom com IdCupon: {idCupon}.");
                throw;
            }
        }
    }
}
