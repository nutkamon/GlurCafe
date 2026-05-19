using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class ProductCost
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อสินค้า")]
        [StringLength(200)]
        [Display(Name = "ชื่อสินค้า")]
        public string ProductName { get; set; } = "";

        [Display(Name = "อัตราสูญหาย (%)")]
        public decimal LossRate { get; set; } = 20;

        [Display(Name = "ค่าดำเนินการ (฿/กก.)")]
        public decimal ProcessingFee { get; set; } = 50;

        [Display(Name = "ต้นทุน/กก. (฿)")]
        public decimal CostPerKg { get; set; }

        [Display(Name = "ราคาขาย/กก. (฿)")]
        public decimal SellingPrice { get; set; }

        [Display(Name = "กำไร (%)")]
        public decimal ProfitPercent { get; set; }

        [StringLength(500)]
        [Display(Name = "หมายเหตุ")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public List<ProductCostItem> Items { get; set; } = new();
    }
}
