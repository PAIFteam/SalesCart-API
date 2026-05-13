using SalesCart.Core.Domain.Entities;


namespace SalesCart.Core.Domain.Interfaces
{
    public interface ISalesCartUserRepository
    {
        Task<SalesCartUser> GetSalesCartUserByIdUserAsync(int idUser);
        Task PutSalesCartUserAsync(SalesCartUser salesCartUser);
        Task DeleteSalesCartUserAsync(int idUser);
    }
}
