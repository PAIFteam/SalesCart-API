using SalesCart.Core.Domain.Entities;

namespace SalesCart.Core.Domain.Interfaces
{
    public interface ICuponRepository
    {
        Task<Cupon> GetCuponByIdAsync(int idCupon);
        Task<IEnumerable<Cupon>> GetAllCuponesAsync();
        Task<int> CreateCuponAsync(Cupon cupon);
        Task<bool> UpdateCuponAsync(Cupon cupon);
        Task<bool> DeleteCuponAsync(int idCupon);
    }
}
