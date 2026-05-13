using SalesCart.Core.Domain.Entities;
using SalesCart.Core.Domain.Interfaces;
using SalesCart.Infra.Cache;
using Microsoft.Extensions.Logging;

namespace SalesCart.Infra.Services
{
    public class CuponValidationService : ICuponValidationService
    {
        private readonly ICuponRepository _cuponRepository;
        private readonly IRedisService _redisService;
        private readonly ILogger<CuponValidationService> _logger;
        private const string CACHE_KEY = "cupons:all";

        public CuponValidationService(
            ICuponRepository cuponRepository,
            IRedisService redisService,
            ILogger<CuponValidationService> logger)
        {
            _cuponRepository = cuponRepository;
            _redisService = redisService;
            _logger = logger;
        }

        public async Task<bool> IsValidCuponNameAsync(string cuponName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cuponName))
                {
                    _logger.LogWarning("Nome do cupom vazio ou nulo para validação.");
                    return false;
                }

                var cupom = await GetMatchingCuponAsync(cuponName);
                return cupom != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao validar cupom para o nome: {cuponName}");
                throw;
                   
            }
        }

        public async Task<Cupon?> GetMatchingCuponAsync(string cuponName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cuponName))
                {
                    return null;
                }

                // Tentar buscar cupons do cache
                var cupones = await _redisService.GetAsync<List<Cupon>>(CACHE_KEY);

                // Se não estiver em cache, buscar do repositório (que buscará e colocará em cache)
                if (cupones == null || !cupones.Any())
                {
                    cupones = (await _cuponRepository.GetAllCuponesAsync()).ToList();
                }

                // Buscar cupom que corresponda ao nome do usuário (case-insensitive)
                var matchingCupon = cupones.FirstOrDefault(c => 
                    c.CuponName != null && 
                    c.CuponName.Equals(cuponName, StringComparison.OrdinalIgnoreCase) &&
                    (c.Ativo == true || c.Ativo == null)); // Considerar como válido se ativo for true ou null

                if (matchingCupon != null)
                {
                    _logger.LogInformation($"Cupom encontrado para o nome de usuário '{cuponName}': {matchingCupon.CuponName}");
                }
                else
                {
                    _logger.LogInformation($"Nenhum cupom encontrado para o nome de usuário '{cuponName}'.");
                }

                return matchingCupon;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar cupom correspondente para o nome de usuário: {cuponName}");
                throw;
            }
        }
    }
}
