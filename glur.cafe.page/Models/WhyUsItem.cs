using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class WhyUsItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Bootstrap Icon Class")]
        public string IconClassName { get; set; } = "";

        [Required(ErrorMessage = "กรุณากรอกหัวข้อ")]
        [StringLength(100)]
        [Display(Name = "หัวข้อ")]
        public string Title { get; set; } = "";

        [StringLength(300)]
        [Display(Name = "คำอธิบาย")]
        public string Description { get; set; } = "";

        [Display(Name = "ลำดับการแสดงผล")]
        public int DisplayOrder { get; set; }

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;
    }
}
