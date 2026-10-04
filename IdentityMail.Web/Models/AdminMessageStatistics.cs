using IdentityMail.Web.Entites.Enums;

namespace IdentityMail.Web.Models
{
    public class AdminMessageStatistics
    {
        public int Id { get; set; }
        public string ReceiverEmail { get; set; }
        public string SenderEmail { get; set; }
        public string ReceiverProfileImageUrl { get; set; }
        public string SenderProfileImageUrl { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Category { get; set; }
        public CategoryTheme CategoryTheme { get; set; }
        public DateTime SendDate { get; set; }
    }
}
