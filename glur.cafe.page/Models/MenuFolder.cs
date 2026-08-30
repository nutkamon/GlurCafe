using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class MenuFolder
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อหมวดหมู่")]
        [StringLength(100)]
        [Display(Name = "ชื่อหมวดหมู่")]
        public string Name { get; set; } = "";

        [StringLength(50)]
        [Display(Name = "Bootstrap Icon Class")]
        public string IconClassName { get; set; } = "bi-cup-hot";

        [Display(Name = "ลำดับการแสดงผล")]
        public int DisplayOrder { get; set; }

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<MenuItem> Items { get; set; } = new();
    }
}
