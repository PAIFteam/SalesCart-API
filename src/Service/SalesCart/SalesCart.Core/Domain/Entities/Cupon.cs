namespace SalesCart.Core.Domain.Entities
{
    public class Cupon
    {
        public int IdCupon { get; set; }

        public string CuponName { get; set; } = string.Empty;

        public decimal PercentDesc { get; set; }

        public DateTime DataStart { get; set; }

        public DateTime DataEnd { get; set; }

        public int? UsageLimit { get; set; }

        public int? UsageQuantity { get; set; }

        public bool? Ativo { get; set; }
    }
}
