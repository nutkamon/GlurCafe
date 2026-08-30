using System.ComponentModel.DataAnnotations;

namespace glur.cafe.page.Models
{
    public class MessengerMessage
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }
        public MessengerConversation Conversation { get; set; } = null!;

        /// <summary>Facebook message mid (for deduplication)</summary>
        [StringLength(100)]
        public string? MessageId { get; set; }

        /// <summary>false = message from customer, true = message sent by page/admin</summary>
        public bool IsFromPage { get; set; } = false;

        [StringLength(2000)]
        public string? Text { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}
