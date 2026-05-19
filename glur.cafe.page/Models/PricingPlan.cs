using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class PricingPlan
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อแพ็คเกจ")]
        [StringLength(100)]
        [Display(Name = "ชื่อแพ็คเกจ")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "กรุณากรอกราคา")]
        [StringLength(50)]
        [Display(Name = "ราคา")]
        public string Price { get; set; } = "";

        [StringLength(50)]
        [Display(Name = "หน่วยราคา")]
        public string PriceUnit { get; set; } = "";

        [StringLength(300)]
        [Display(Name = "คำอธิบาย")]
        public string Description { get; set; } = "";

        /// <summary>
        /// JSON array string e.g. ["feature1","feature2"]
        /// </summary>
        [Display(Name = "คุณสมบัติ (JSON)")]
        public string FeaturesJson { get; set; } = "[]";

        [Display(Name = "ยอดนิยม")]
        public bool IsPopular { get; set; }

        [StringLength(50)]
        [Display(Name = "ข้อความปุ่ม CTA")]
        public string CtaText { get; set; } = "สั่งซื้อเลย";

        [Display(Name = "ลำดับการแสดงผล")]
        public int DisplayOrder { get; set; }

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
