using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อบริการ")]
        [StringLength(100)]
        [Display(Name = "ชื่อบริการ")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "กรุณากรอกคำอธิบาย")]
        [StringLength(500)]
        [Display(Name = "คำอธิบาย")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "กรุณากรอก Icon Class")]
        [StringLength(50)]
        [Display(Name = "Bootstrap Icon Class")]
        public string IconClassName { get; set; } = "";

        [Required]
        [StringLength(20)]
        [Display(Name = "Gradient CSS Class")]
        public string GradientClass { get; set; } = "grad-1";

        [StringLength(300)]
        [Display(Name = "รูปภาพ (path)")]
        public string? ImagePath { get; set; }

        [Display(Name = "ลำดับการแสดงผล")]
        public int DisplayOrder { get; set; }

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
