using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class MessengerConversation
    {
        public int Id { get; set; }

        /// <summary>Facebook Page-Scoped User ID</summary>
        [Required]
        [StringLength(50)]
        public string PsId { get; set; } = "";

        [StringLength(100)]
        public string DisplayName { get; set; } = "ผู้ใช้ Facebook";

        [StringLength(500)]
        public string? LastMessageText { get; set; }

        public DateTime LastMessageAt { get; set; } = DateTime.Now;

        public int UnreadCount { get; set; } = 0;

        /// <summary>Linked CRM Customer (optional)</summary>
        public int? CustomerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<MessengerMessage> Messages { get; set; } = new List<MessengerMessage>();
    }
}
