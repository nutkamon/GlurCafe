using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อลูกค้า")]
        [StringLength(100)]
        [Display(Name = "ชื่อ-นามสกุล")]
        public string FullName { get; set; } = "";

        [StringLength(20)]
        [Display(Name = "เบอร์โทร")]
        public string? Phone { get; set; }

        [StringLength(100)]
        [Display(Name = "อีเมล")]
        public string? Email { get; set; }

        [StringLength(100)]
        [Display(Name = "บริษัท / ร้านค้า")]
        public string? Company { get; set; }

        [StringLength(500)]
        [Display(Name = "ที่อยู่")]
        public string? Address { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "ประเภทลูกค้า")]
        public string CustomerType { get; set; } = "individual";
        // individual, business, reseller

        [Required]
        [StringLength(20)]
        [Display(Name = "สถานะ")]
        public string Status { get; set; } = "lead";
        // lead, prospect, active, inactive, vip

        [StringLength(30)]
        [Display(Name = "แหล่งที่มา")]
        public string? Source { get; set; }
        // contact_form, quotation, walk_in, referral, other

        [StringLength(2000)]
        [Display(Name = "หมายเหตุ")]
        public string? Notes { get; set; }

        [StringLength(200)]
        [Display(Name = "แท็ก")]
        public string? Tags { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? LastContactedAt { get; set; }

        public List<CustomerInteraction> Interactions { get; set; } = new();
        public List<Quotation> Quotations { get; set; } = new();
        public List<ContactMessage> ContactMessages { get; set; } = new();
    }
}
