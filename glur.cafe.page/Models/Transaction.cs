using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace glur.cafe.page.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Type { get; set; } = "income"; // "income" | "expense"

        [Required(ErrorMessage = "กรุณาระบุหมวดหมู่")]
        [StringLength(50)]
        [Display(Name = "หมวดหมู่")]
        public string Category { get; set; } = "";

        [Required(ErrorMessage = "กรุณากรอกรายละเอียด")]
        [StringLength(200)]
        [Display(Name = "รายละเอียด")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "กรุณากรอกจำนวนเงิน")]
        [Column(TypeName = "decimal(12,2)")]
        [Display(Name = "จำนวนเงิน")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "วันที่")]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(500)]
        [Display(Name = "หมายเหตุ")]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
