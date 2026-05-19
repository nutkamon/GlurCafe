using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อ")]
        [StringLength(100)]
        [Display(Name = "ชื่อ-นามสกุล")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "กรุณากรอกเบอร์โทร")]
        [StringLength(20)]
        [Display(Name = "เบอร์โทรศัพท์")]
        public string Phone { get; set; } = "";

        [StringLength(100)]
        [Display(Name = "อีเมล")]
        public string? Email { get; set; }

        [StringLength(100)]
        [Display(Name = "ประเภทที่สนใจ")]
        public string? ServiceType { get; set; }

        [StringLength(2000)]
        [Display(Name = "รายละเอียด")]
        public string? Message { get; set; }

        [Display(Name = "อ่านแล้ว")]
        public bool IsRead { get; set; } = false;

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
