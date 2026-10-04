using IdentityMail.Web.Entites.Enums;

namespace IdentityMail.Web.Entites
{
    public class MessageCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public bool IsSystemProtected { get; set; }
        public CategoryTheme Theme { get; set; }
    }
}
