using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class SaleOrderItem
    {
        public int Id { get; set; }

        public int SaleOrderId { get; set; }

        [StringLength(200)]
        public string Description { get; set; } = "";

        public decimal Quantity { get; set; } = 1;

        [StringLength(20)]
        public string Unit { get; set; } = "กก.";

        public decimal UnitPrice { get; set; }

        public decimal Amount { get; set; }

        public SaleOrder? SaleOrder { get; set; }
    }
}
