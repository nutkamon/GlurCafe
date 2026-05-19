using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class AdminUser
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "ชื่อผู้ใช้")]
        public string Username { get; set; } = "";

        [Required]
        [StringLength(200)]
        public string PasswordHash { get; set; } = "";

        [StringLength(100)]
        [Display(Name = "ชื่อที่แสดง")]
        public string DisplayName { get; set; } = "";

        [Display(Name = "เปิดใช้งาน")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
    }
}
