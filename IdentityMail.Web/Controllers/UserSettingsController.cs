using IdentityMail.Web.DTOs.UserDtos;
using IdentityMail.Web.Entites;
using IdentityMail.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdentityMail.Web.Controllers
{
    [Authorize]
    public class UserSettingsController(UserManager<AppUser> _userManager,
                                        IWebHostEnvironment _webHostEnvironment) : Controller
    {
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);

            var viewModel = new UserSettingsViewModel
            {
                appUser = user,
                ChangePassword = new ChangePasswordDto()
            };

            ViewBag.fullName = user.FirstName + " " + user.LastName;
            ViewBag.ActiveTab = "Settings";

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePhoto(IFormFile profileImage)
        {
            // Dosya boş mu değil mi
            if (profileImage == null || profileImage.Length == 0)
            {
                TempData["ImageErrorMessage"] = "Lütfen Geçerli Bir Dosya Seçin";
                return RedirectToAction("Settings");
            }

            // Sadece Resim Dosyalarına izin ver
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["ImageErrorMessage"] = "Sadece JPG ve PNG formatları desteklenmektedir.";
                return RedirectToAction("Settings");
            }

            // Aktif Kullanıcıyı bul
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            // Kullanıcının bir fotoğrafı var mı ve bu fotoğraf varsayılan değil mi kontrol et
            if (!string.IsNullOrEmpty(user.ProfileImageUrl) && !user.ProfileImageUrl.Contains("default-avatar"))
            {
                // sunucudaki fiziksel dosya yolunu bul
                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));

                // Dosya gerçekten diskte duruyorsa fiziksel olarak sil
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Kaydedilecek klasör yolunu belirle
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");

            // Klasör yoksa oluştur
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Dosya isminin benzersiz olması için Guid ekle
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + profileImage.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Dosyayı Sunucuya fiziksel olarak kopyala
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(fileStream);
            }

            // Sunucuda yer kaplamaması için varsa eski fotoğrafı sil
            if (!string.IsNullOrEmpty(user.ProfileImageUrl) && !user.ProfileImageUrl.Contains("default-avatar"))
            {
                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Veritabanında kullanıcının fotoğraf yolunu güncelle
            user.ProfileImageUrl = "/images/avatars/" + uniqueFileName;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["ImageErrorMessage"] = "Fotoğraf güncellenirken bir hata oluştu.";
            }
            else
            {
                TempData["ImageSuccessMessage"] = "Profil fotoğrafınız başarıyla güncellendi.";
            }



            return RedirectToAction("Settings");
        }

        [HttpPost]
        public async Task<IActionResult> RemovePhoto()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            // Kullanıcının bir fotoğrafı var mı ve bu fotoğraf varsayılan değil mi kontrol et
            if (!string.IsNullOrEmpty(user.ProfileImageUrl) && !user.ProfileImageUrl.Contains("default-avatar"))
            {
                // sunucudaki fiziksel dosya yolunu bul
                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));

                // Dosya gerçekten diskte duruyorsa fiziksel olarak sil
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Veritabanında fotoğraf yolunu sıfırla (null veya varsayılan bir path ver)
            user.ProfileImageUrl = "/images/avatars/default-avatar.jpg";

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["ImageErrorMessage"] = "Fotoğraf silinirken bir hata oluştu.";
            }
            else
            {
                TempData["ImageSuccessMessage"] = "Profil fotoğrafınız başarıyla Kaldırıldı.";
            }

            return RedirectToAction("Settings");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInfos(UserSettingsViewModel model)
        {


            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            user.FirstName = model.appUser.FirstName;
            user.LastName = model.appUser.LastName;
            user.UserName = model.appUser.UserName;
            user.PhoneNumber = model.appUser.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["InfoErrorMessage"] = "Güncelleme sırasında bir hata oluştu.";

                // HATA DURUMUNDA: Sayfanın patlamaması için ViewModel'i eksiksiz geri dolduruyoruz
                model.appUser = user;
                model.ChangePassword = new ChangePasswordDto();

                return View("Settings", model);
            }
            else
            {
                TempData["InfoSuccessMessage"] = "Kişisel bilgileriniz başarıyla güncellendi.";
            }

            return RedirectToAction("Settings");
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UserSettingsViewModel model)
        {
            var changePasswordDto = model.ChangePassword;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Şifreler Aynı Değil !");

                model.appUser = user;
                return View("Settings", model);
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                model.appUser = user;
                return View("Settings", model);
            }

            TempData["PasswordSuccessMessage"] = "Şifreniz Başarıyla Güncellendi";
            return RedirectToAction("Settings");
        }
    }
}
