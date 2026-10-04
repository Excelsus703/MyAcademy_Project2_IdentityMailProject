using IdentityMail.Web.Entites.Enums;

namespace IdentityMail.Web.Models
{
    public class CategoryListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public CategoryTheme Theme { get; set; }
        public int UnreadCount { get; set; }
    }
}
