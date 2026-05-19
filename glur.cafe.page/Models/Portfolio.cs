using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class Portfolio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อรายการ")]
        [StringLength(100)]
        [Display(Name = "ชื่อรายการ")]
        public string ProjectName { get; set; } = "";

        [StringLength(100)]
        [Display(Name = "รายละเอียดเพิ่มเติม")]
        public string? ClientName { get; set; }

        [Required(ErrorMessage = "กรุณากรอกหมวดหมู่")]
        [StringLength(50)]
        [Display(Name = "หมวดหมู่")]
        public string Category { get; set; } = "";

        [StringLength(300)]
        [Display(Name = "รูปภาพ (path)")]
        public string? ImagePath { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Bootstrap Icon Class")]
        public string IconClassName { get; set; } = "";

        [Required]
        [StringLength(20)]
        [Display(Name = "Gradient CSS Class")]
        public string GradientClass { get; set; } = "port-1";

        [StringLength(500)]
        [Display(Name = "คำอธิบาย")]
        public string? Description { get; set; }

        [Display(Name = "ลำดับการแสดงผล")]
        public int DisplayOrder { get; set; }

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
