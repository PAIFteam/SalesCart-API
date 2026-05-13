
namespace SalesCart.Core.Domain.Entities
{ 
    public class SalesCartUser
    {
        
        public int IdUser { get; set; }
       
        public decimal PriceTotal { get; set; }

        public List<SalesCartItem> Items { get; set; } = new List<SalesCartItem>();

        public string CuponName { get; set; } = string.Empty;

    } 
}
