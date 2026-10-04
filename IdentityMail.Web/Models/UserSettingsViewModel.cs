using IdentityMail.Web.DTOs.UserDtos;
using IdentityMail.Web.Entites;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace IdentityMail.Web.Models
{
    public class UserSettingsViewModel
    {
        [ValidateNever]
        public AppUser appUser { get; set; }

        public ChangePasswordDto ChangePassword { get; set; }
    }
}
