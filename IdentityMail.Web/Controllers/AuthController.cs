using IdentityMail.Web.DTOs.UserDtos;
using IdentityMail.Web.Entites;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace IdentityMail.Web.Controllers
{
    public class AuthController(UserManager<AppUser> _userManager,
                                SignInManager<AppUser> _signInManager,
                                RoleManager<AppRole> _roleManager) : Controller
    {

        //private readonly UserManager<AppUser> _userManager;

        //public AuthController(UserManager<AppUser> userManager)
        //{
        //    _userManager = userManager;
        //}

        // Admin Kullanıcı Adı auth@cybermail.com
        // Admin Şifresi Password12*


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Şifreler Birbiri ile Uyumlu Değil");
                return View(registerDto);
            }

            var user = new AppUser
            {
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.UserName,
                ProfileImageUrl = "/images/avatars/default-avatar.jpg"
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return View(registerDto);
            }

            await _userManager.AddToRoleAsync(user, "STANDART KULLANICI");

            return RedirectToAction("Verify", new { newUserEmail = user.Email });
        }

        public IActionResult Verify(string newUserEmail)
        {
            ViewBag.NewUserEmail = newUserEmail;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendCode(string email)
        {
            var newUser = await _userManager.FindByEmailAsync(email);
            if (newUser == null)
            {
                return RedirectToAction("Register");
            }

            Random random = new Random();
            int code = random.Next(100000, 1000000);

            newUser.ConfirmCode = code;
            await _userManager.UpdateAsync(newUser);


            MimeMessage mimeMessage = new MimeMessage();
            MailboxAddress mailboxAddressFrom = new MailboxAddress("CyberMail Admin", "mustafa70310@gmail.com");
            MailboxAddress mailboxAddressTo = new MailboxAddress("User", newUser.Email);

            mimeMessage.From.Add(mailboxAddressFrom);
            mimeMessage.To.Add(mailboxAddressTo);

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = "Kayıt işlemini gerçekleştirmek için onay kodunuz: " + newUser.ConfirmCode;
            mimeMessage.Body = bodyBuilder.ToMessageBody();
            mimeMessage.Subject = "CyberMail Onay Kodu";

            using (SmtpClient client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync("mustafa70310@gmail.com", "ckqu smod qwsx zejl");
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }


            return RedirectToAction("VerifyEmail", new { email = newUser.Email });
        }

        public async Task<IActionResult> VerifyEmail(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(string email, string[] code)
        {
            string joinedCode = string.Join("", code);
            int.TryParse(joinedCode, out var inputCode);

            var user = await _userManager.FindByEmailAsync(email);

            if (user != null && user.ConfirmCode == inputCode)
            {
                user.EmailConfirmed = true;
                user.ConfirmCode = null;

                await _userManager.UpdateAsync(user);

                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Girdiğiniz Doğrulama Kodu Hatalı !");
            ViewBag.Email = email;

            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Bu Email sistemde kayıtlı değil!");
                return View(loginDto);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Email veya Şifre hatalı !");
                return View(loginDto);
            }

            return RedirectToAction("Index", "Message");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }
    }
}
