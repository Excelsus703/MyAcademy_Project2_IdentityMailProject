using IdentityMail.Web.Entites.Enums;

namespace IdentityMail.Web.Models
{
    public class AdminCategoryListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public bool IsSystemProtected { get; set; }
        public int ColorTheme { get; set; }
        public int MessageCount { get; set; }
    }
}
