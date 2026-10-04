namespace IdentityMail.Web.Models
{
    public class UserListViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsPassive { get; set; }
        public int SentMessageCount { get; set; }
        public string RoleName { get; set; }
    }
}
