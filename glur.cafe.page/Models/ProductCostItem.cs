using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class ProductCostItem
    {
        public int Id { get; set; }

        public int ProductCostId { get; set; }

        public int? BeanTypeId { get; set; }

        [StringLength(100)]
        [Display(Name = "ชื่อสาร")]
        public string BeanName { get; set; } = "";

        [Display(Name = "ราคา/กก. (฿)")]
        public decimal PricePerKg { get; set; }

        [Display(Name = "อัตราส่วน (%)")]
        public decimal RatioPercent { get; set; }

        public ProductCost? ProductCost { get; set; }
        public BeanType? BeanType { get; set; }
    }
}
