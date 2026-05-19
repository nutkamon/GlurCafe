using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class Quotation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "เลขที่ใบเสนอราคา")]
        public string QuotationNumber { get; set; } = "";

        [Required(ErrorMessage = "กรุณากรอกชื่อลูกค้า")]
        [StringLength(100)]
        [Display(Name = "ชื่อลูกค้า")]
        public string CustomerName { get; set; } = "";

        [StringLength(20)]
        [Display(Name = "เบอร์โทร")]
        public string? Phone { get; set; }

        [StringLength(100)]
        [Display(Name = "อีเมล")]
        public string? Email { get; set; }

        [StringLength(500)]
        [Display(Name = "ที่อยู่")]
        public string? Address { get; set; }

        [Display(Name = "รายละเอียดเพิ่มเติม")]
        [StringLength(2000)]
        public string? Notes { get; set; }

        [Display(Name = "ยอดรวม")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "ส่วนลด (%)")]
        public decimal DiscountPercent { get; set; }

        [Display(Name = "ยอดสุทธิ")]
        public decimal NetAmount { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "สถานะ")]
        public string Status { get; set; } = "draft";
        // draft, sent, accepted, rejected, paid, completed, cancelled

        [Display(Name = "สถานะการชำระเงิน")]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "unpaid";
        // unpaid, partial, paid

        [Display(Name = "ยอดชำระแล้ว")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "วิธีชำระเงิน")]
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [Display(Name = "หมายเหตุการชำระเงิน")]
        [StringLength(500)]
        public string? PaymentNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? PaidAt { get; set; }

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public List<QuotationItem> Items { get; set; } = new();
    }
}
