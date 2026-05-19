using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class ProcessStep
    {
        public int Id { get; set; }

        [Display(Name = "ลำดับขั้นตอน")]
        public int StepNumber { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Bootstrap Icon Class")]
        public string IconClassName { get; set; } = "";

        [Required(ErrorMessage = "กรุณากรอกชื่อขั้นตอน")]
        [StringLength(100)]
        [Display(Name = "ชื่อขั้นตอน")]
        public string Title { get; set; } = "";

        [StringLength(300)]
        [Display(Name = "คำอธิบาย")]
        public string Description { get; set; } = "";

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;
    }
}
