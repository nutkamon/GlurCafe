using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class StatItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "ค่า (เช่น 5+, 50+)")]
        public string Value { get; set; } = "";

        [Required]
        [StringLength(50)]
        [Display(Name = "ป้ายกำกับ")]
        public string Label { get; set; } = "";

        [Required]
        [StringLength(50)]
        [Display(Name = "Bootstrap Icon Class")]
        public string IconClassName { get; set; } = "";

        [Display(Name = "ลำดับการแสดงผล")]
        public int DisplayOrder { get; set; }

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;
    }
}
