using Microsoft.Extensions.Logging;
using SalesCart.Core.Domain.Entities.Base;
using SalesCart.Core.Domain.Interfaces;

namespace SalesCart.Core.Application.UseCases.SalesCartUser
{
    public class SalesCartUserUseCase
    {
        private readonly ISalesCartUserRepository _salesCartUserRepository;
        private readonly ICuponValidationService _cuponValidationService;
        private readonly ILogger<SalesCartUserUseCase> _logger;

        public SalesCartUserUseCase(
            ISalesCartUserRepository salesCartUserRepository,
            ICuponValidationService cuponValidationService,
            ILogger<SalesCartUserUseCase> logger
        )
        {
            _salesCartUserRepository = salesCartUserRepository;
            _cuponValidationService = cuponValidationService;
            _logger = logger;

        }

        public async Task<SalesCartUserOutput> PutSalesCartUserAsync(SalesCartUserInput input)
        {
            _logger.LogInformation("Starting PutGamerUserCase.ExecuteAsync");
            _logger.LogInformation($"Atualização de carrinho de compras do Usuario: {input.IdUser}");

            try
            {                
                OutPutBase outPutBase = ValidateInput(input);

                if (!outPutBase.Result)
                {
                    return new SalesCartUserOutput
                    {
                        Result = false,
                        Message = outPutBase.Message,
                        Exception = outPutBase.Exception
                    };
                }

                // Inicializar preço total com o valor fornecido
                decimal finalPrice = input.PriceTotal;

                // Validar e aplicar cupom se fornecido
                if (!string.IsNullOrWhiteSpace(input.CuponName))
                {
                    _logger.LogInformation($"Validando cupom: {input.CuponName}");

                    bool isCuponValid = await _cuponValidationService.IsValidCuponNameAsync(input.CuponName);

                    if (isCuponValid)
                    {
                        var matchingCupon = await _cuponValidationService.GetMatchingCuponAsync(input.CuponName);

                        if (matchingCupon != null && matchingCupon.PercentDesc > 0)
                        {
                            // Calcular desconto
                            decimal discountAmount = finalPrice * (matchingCupon.PercentDesc / 100);
                            finalPrice = finalPrice - discountAmount;

                            _logger.LogInformation($"Cupom '{input.CuponName}' aplicado com sucesso. Desconto: {matchingCupon.PercentDesc}%. Preço original: {input.PriceTotal}, Preço com desconto: {finalPrice}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Cupom '{input.CuponName}' não é válido ou não foi encontrado.");
                    }
                }

                await _salesCartUserRepository.PutSalesCartUserAsync(new Domain.Entities.SalesCartUser
                {
                    IdUser = input.IdUser,
                    PriceTotal = finalPrice,
                    Items = input.Items,
                    CuponName = input.CuponName

                });

                _logger.LogInformation($"Carrinho de Compras atualiado para o Usuario: {input.IdUser}");

                SalesCartUserOutput outPut = new SalesCartUserOutput
                {
                    Result = true,
                    Message = "Carrinho de compras atualizado com sucesso!",
                    Exception = null

                };

                return outPut;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao atualizar o carrinho de compras");
                return new SalesCartUserOutput
                {
                    Result = false,
                    Message = "Ocorreu umm erro de Runtime Interno",
                    Exception = ex
                };

            }

        }
        public async Task<Domain.Entities.SalesCartUser> GetSalesCartUserAsync(int idUser)
        {
            _logger.LogInformation("Starting GetSalesCartUserAsync");
            _logger.LogInformation($"Recuperando carrinho de compras do Usuario: {idUser}");

            try
            {
                var salesCartUser = await _salesCartUserRepository.GetSalesCartUserByIdUserAsync(idUser);
                return salesCartUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao recuperar o carrinho de compras");
                throw;
            }        

        }

        public async Task<Domain.Entities.SalesCartUser> ClearSalesCartUserAsync(int idUser)
        {
            _logger.LogInformation("Starting ClearSalesCartUserAsync");
            _logger.LogInformation($"Limpando carrinho de compras do Usuario: {idUser}");

            try
            {
                await _salesCartUserRepository.DeleteSalesCartUserAsync(idUser);
                var salesCartUser = new Domain.Entities.SalesCartUser {
                                    IdUser = idUser, 
                                    PriceTotal = 0, 
                                    Items = new List<Domain.Entities.SalesCartItem>()
                };
                return salesCartUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao recuperar o carrinho de compras");
                throw;
            }

        }

        private OutPutBase ValidateInput(SalesCartUserInput input)
        {
            //Validações de entrada

            OutPutBase outPut = new OutPutBase();
            outPut.Result = true;
            // Implement validation logic here
            return outPut;
        }
    }
}
