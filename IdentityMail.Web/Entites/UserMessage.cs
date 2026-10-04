namespace IdentityMail.Web.Entites
{
    public class UserMessage
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SendDate { get; set; }
        public bool isRead { get; set; }
        public bool isImportant { get; set; }
        public bool isTrash { get; set; } = false;
        public bool isDraft { get; set; } = false;
        public bool IsComplaint { get; set; }
        public bool IsComplaintAccepted { get; set; }
        public bool IsComplaintFalse { get; set; }
        public bool IsComplaintResolved { get; set; }
        public string? ComplaintReason { get; set; }


        public int? MessageCategoryId { get; set; }
        public MessageCategory? MessageCategory { get; set; }

        public AppUser Sender { get; set; }
        public int SenderId { get; set; }

        public AppUser Receiver { get; set; }
        public int ReceiverId { get; set; }

        public ICollection<MessageAttachment> MessageAttachments { get; set; } = new List<MessageAttachment>();
    }
}
