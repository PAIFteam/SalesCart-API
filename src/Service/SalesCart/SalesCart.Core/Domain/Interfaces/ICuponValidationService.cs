using SalesCart.Core.Domain.Entities;

namespace SalesCart.Core.Domain.Interfaces
{
    public interface ICuponValidationService
    {
        Task<bool> IsValidCuponNameAsync(string userName);
        Task<Cupon?> GetMatchingCuponAsync(string userName);
    }
}
