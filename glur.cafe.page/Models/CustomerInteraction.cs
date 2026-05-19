using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class CustomerInteraction
    {
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        [Required]
        [StringLength(20)]
        [Display(Name = "ประเภทการติดต่อ")]
        public string Type { get; set; } = "note";
        // call, email, meeting, line, visit, order, note

        [StringLength(200)]
        [Display(Name = "หัวข้อ")]
        public string? Subject { get; set; }

        [StringLength(2000)]
        [Display(Name = "รายละเอียด")]
        public string? Notes { get; set; }

        [Display(Name = "วันที่นัดติดตาม")]
        public DateTime? FollowUpDate { get; set; }

        [Display(Name = "ติดตามแล้ว")]
        public bool IsFollowUpDone { get; set; } = false;

        [StringLength(100)]
        [Display(Name = "บันทึกโดย")]
        public string? CreatedByAdmin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
