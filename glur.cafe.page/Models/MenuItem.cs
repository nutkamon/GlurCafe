using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace glur.cafe.page.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        public int FolderId { get; set; }
        public MenuFolder Folder { get; set; } = null!;

        [Required]
        [StringLength(300)]
        [Display(Name = "ไฟล์ (path)")]
        public string FilePath { get; set; } = "";

        /// <summary>"image" หรือ "video"</summary>
        [Required]
        [StringLength(20)]
        public string MediaType { get; set; } = "image";

        [StringLength(100)]
        [Display(Name = "ชื่อเมนู (ไม่บังคับ)")]
        public string? Title { get; set; }

        [Column(TypeName = "TEXT")]
        [Display(Name = "ราคา (ไม่บังคับ)")]
        public decimal? Price { get; set; }

        [Display(Name = "เมนูแนะนำ")]
        public bool IsRecommended { get; set; }

        [StringLength(500)]
        [Display(Name = "รายละเอียด")]
        public string? Description { get; set; }

        [StringLength(500)]
        [Display(Name = "ตัวเลือก / ขนาด")]
        public string? Options { get; set; }

        [Display(Name = "ลำดับการแสดงผล")]
        public int DisplayOrder { get; set; }

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
