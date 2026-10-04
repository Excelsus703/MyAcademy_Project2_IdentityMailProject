namespace IdentityMail.Web.Models
{
    public class MessageSearchFilterViewModel
    {
        public string SearchTerm { get; set; }
        public string SearchType { get; set; } = "all";
        public int? CategoryId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class QuickSearchResultViewModel
    {
        public int Id { get; set; }
        public string SenderFullName { get; set; }
        public string Subject { get; set; }
        public string DateStr { get; set; }
    }
}
