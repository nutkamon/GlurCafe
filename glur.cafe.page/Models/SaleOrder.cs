using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class SaleOrder
    {
        public int Id { get; set; }

        [StringLength(30)]
        public string OrderNumber { get; set; } = "";

        [Required]
        public DateTime Date { get; set; } = DateTime.Today;

        public decimal ItemsTotal { get; set; }

        public decimal DeliveryFee { get; set; }

        public decimal GrandTotal { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<SaleOrderItem> Items { get; set; } = new();
    }
}
