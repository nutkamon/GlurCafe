using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class BeanType
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อสาร")]
        [StringLength(100)]
        [Display(Name = "ชื่อสาร")]
        public string Name { get; set; } = "";

        [Display(Name = "ราคา/กก. (฿)")]
        public decimal PricePerKg { get; set; }

        [StringLength(500)]
        [Display(Name = "หมายเหตุ")]
        public string? Notes { get; set; }

        [Display(Name = "ใช้งาน")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
