using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class SiteSetting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Key { get; set; } = "";

        [Required]
        [StringLength(500)]
        [Display(Name = "ค่า")]
        public string Value { get; set; } = "";

        [StringLength(50)]
        [Display(Name = "กลุ่ม")]
        public string Group { get; set; } = "General";

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
