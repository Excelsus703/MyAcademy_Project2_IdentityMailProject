using IdentityMail.Web.Entites;
using IdentityMail.Web.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.Threading.Tasks;

namespace IdentityMail.Web.Controllers
{
    public class PasswordChangeController(UserManager<AppUser> _userManager) : Controller
    {
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel forgetPasswordViewModel)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordViewModel.Mail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Yazmış Olduğunuz E-Posta Adresini Bulamadık !");

                return View();
            }

            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResetTokenLink = Url.Action("ResetPassword", "PasswordChange", new
            {
                userId = user.Id,
                Token = passwordResetToken
            }, HttpContext.Request.Scheme);


            MimeMessage mimeMessage = new MimeMessage();
            MailboxAddress mailboxAddressFrom = new MailboxAddress("CyberMail Admin", "mustafa70310@gmail.com");
            MailboxAddress mailboxAddressTo = new MailboxAddress("User", forgetPasswordViewModel.Mail);

            mimeMessage.From.Add(mailboxAddressFrom);
            mimeMessage.To.Add(mailboxAddressTo);

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = passwordResetTokenLink;
            mimeMessage.Body = bodyBuilder.ToMessageBody();
            mimeMessage.Subject = "Şifre Değişiklik Talebi";

            using (SmtpClient client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync("mustafa70310@gmail.com", "ckqu smod qwsx zejl");
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }


            if (passwordResetToken != null)
            {
                ViewBag.SucceedState = true;
            }

            ViewBag.UserMail = user;
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string userid, string token)
        {
            if (userid == null || token == null)
            {
                ViewBag.IsTokenStateNull = true;
            }

            var model = new ResetPasswordViewModel
            {
                UserId = userid,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            var user = await _userManager.FindByIdAsync(resetPasswordViewModel.UserId);

            if (resetPasswordViewModel.Password != resetPasswordViewModel.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Şifreler Aynı Değil");
                return View(resetPasswordViewModel);
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Token, resetPasswordViewModel.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return View(resetPasswordViewModel);
            }

            return RedirectToAction("Login", "Auth");
        }
    }
}
