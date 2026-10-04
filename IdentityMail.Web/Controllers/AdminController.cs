using IdentityMail.Web.Context;
using IdentityMail.Web.Entites;
using IdentityMail.Web.Entites.Enums;
using IdentityMail.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IdentityMail.Web.Controllers
{
    [Authorize(Roles = "Admin,Süper Admin")]
    public class AdminController(UserManager<AppUser> _userManager,
                                 AppDbContext _context,
                                 RoleManager<AppRole> _roleManager) : Controller
    {
        public IActionResult Dashboard()
        {
            var now = DateTime.Now;

            var totalUsers = _context.Users.Count();
            var totalActiveUsers = _context.Users.Where(x => x.IsPassive == false).Count();
            var totalMessages = _context.UserMessages.Where(x => x.isDraft == false && x.isTrash == false).Count();
            var totalMessagesThisMonth = _context.UserMessages.Count(m => m.SendDate >= now.AddDays(-30));
            var totalUnreadMesssages = _context.UserMessages.Where(x => x.isRead == false && x.isDraft == false && x.isTrash == false).Count();
            var totalTrashMessages = _context.UserMessages.Where(x => x.isTrash == true).Count();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalActiveUsers = totalActiveUsers;
            ViewBag.TotalMessages = totalMessages;
            ViewBag.TotalMessagesThisMonth = totalMessagesThisMonth;
            ViewBag.TotalUnreadMessages = totalUnreadMesssages;
            ViewBag.TotalTrashMessages = totalTrashMessages;

            ViewBag.ActiveTab = "Dashboard";

            return View();
        }

        public async Task<IActionResult> Users(int page = 1)
        {
            int pageSize = 10; // Sayfa başına gösterilecek kullanıcı sayısı
            var query = _userManager.Users.AsQueryable();

            int totalUsers = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var userList = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var messageCount = await _context.UserMessages.CountAsync(m => m.SenderId == user.Id);

                userList.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    ProfileImageUrl = user.ProfileImageUrl,
                    IsPassive = user.IsPassive,
                    SentMessageCount = messageCount,
                    RoleName = roles.FirstOrDefault() ?? "ROL YOK"
                });
            }

            ViewBag.ActiveTab = "Users";

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalUsers;

            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalActiveUsers = await _userManager.Users.CountAsync(x => x.IsPassive == false);
            ViewBag.TotalPassiveUsers = await _userManager.Users.CountAsync(x => x.IsPassive == true);
            return View(userList);
        }

        public async Task<IActionResult> Roles()
        {
            var roleCount = _context.Roles.Count();
            ViewBag.RoleCount = roleCount;

            // Rol ID'lerine göre kullanıcı sayılarını sözlük (Dictionary) olarak çekelim
            var usersCountByRoleId = await _context.UserRoles.GroupBy(ur => ur.RoleId)
                                                             .Select(g => new { RoleId = g.Key, Count = g.Count() })
                                                             .ToDictionaryAsync(g => g.RoleId, g => g.Count);

            var roles = await _roleManager.Roles.ToListAsync();

            var values = roles.Select(role => new RoleListViewModel
            {
                Id = role.Id,
                Name = role.Name,
                UserCount = usersCountByRoleId.ContainsKey(role.Id) ? usersCountByRoleId[role.Id] : 0
            }).ToList();

            ViewBag.ActiveTab = "Roles";

            return View(values);
        }

        public async Task<IActionResult> AssignToAdminRole(int id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user != null)
            {
                var userRoles = _context.UserRoles.Where(x => x.UserId == id);
                _context.UserRoles.RemoveRange(userRoles);
                await _context.SaveChangesAsync();

                await _userManager.AddToRoleAsync(user, "ADMIN");
            }

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> RemoveFromAdminRole(int id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user != null)
            {
                var userRoles = _context.UserRoles.Where(x => x.UserId == id);
                _context.UserRoles.RemoveRange(userRoles);
                await _context.SaveChangesAsync();

                await _userManager.AddToRoleAsync(user, "STANDART KULLANICI");
            }

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> UserSetActivePasive(int id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user != null)
            {
                user.IsPassive = !user.IsPassive;

                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Statistics(int page = 1)
        {
            ViewBag.ActiveTab = "Statistics";

            var now = DateTime.Now;

            ViewBag.TotalMessages = await _context.UserMessages.CountAsync();
            ViewBag.Last24Hours = await _context.UserMessages.CountAsync(m => m.SendDate >= now.AddHours(-24));
            ViewBag.ThisWeek = await _context.UserMessages.CountAsync(m => m.SendDate >= now.AddDays(-7));
            ViewBag.ThisMonth = await _context.UserMessages.CountAsync(m => m.SendDate >= now.AddDays(-30));

            int pageSize = 10;
            var query = _context.UserMessages.Include(x => x.Sender)
                                             .Include(x => x.Receiver)
                                             .Include(x => x.MessageCategory)
                                             .OrderByDescending(x => x.SendDate)
                                             .AsQueryable();

            int totalUsers = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            var messages = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var messageList = new List<AdminMessageStatistics>();

            foreach (var message in messages)
            {
                messageList.Add(new AdminMessageStatistics
                {
                    Id = message.Id,
                    ReceiverEmail = message.Receiver.Email,
                    SenderEmail = message.Sender.Email,
                    ReceiverProfileImageUrl = message.Receiver.ProfileImageUrl,
                    SenderProfileImageUrl = message.Sender.ProfileImageUrl,
                    Subject = message.Subject,
                    Body = message.Body,
                    Category = message.MessageCategory.Name,
                    CategoryTheme = message.MessageCategory?.Theme ?? CategoryTheme.WhiteTheme,
                    SendDate = message.SendDate
                });
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(messageList);
        }

        public IActionResult GetMessageDetail(int id)
        {
            // ViewComponent'i çağırıp HTML çıktısını döndürür
            return ViewComponent("AdminMessageDetail", new { id = id });
        }

        public async Task<IActionResult> Complaints(int page = 1)
        {
            ViewBag.ActiveTab = "Complaints";

            ViewBag.TotalComplaints = await _context.UserMessages.Where(x => x.IsComplaint == true).CountAsync();
            ViewBag.TotalAcceptedComplaints = await _context.UserMessages.Where(x => x.IsComplaint == true && x.IsComplaintAccepted == true).CountAsync();
            ViewBag.TotalResolvedComplaints = await _context.UserMessages.Where(x => x.IsComplaint == true && x.IsComplaintResolved == true).CountAsync();
            ViewBag.TotalFalseComplaints = await _context.UserMessages.Where(x => x.IsComplaint == true && x.IsComplaintFalse == true).CountAsync();

            int pageSize = 5;
            var query = _context.UserMessages.Include(x => x.Sender)
                                             .Include(x => x.Receiver)
                                             .Where(x => x.IsComplaint == true)
                                             .AsQueryable();

            int totalUsers = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            var complaints = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(complaints);
        }

        [HttpGet]
        public IActionResult GetComplaintDetail(int id)
        {
            return ViewComponent("AdminComplaintDetail", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateComplaintStatus(int id, string status)
        {
            var message = await _context.UserMessages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            switch (status.ToLower())
            {
                case "accept":
                    message.IsComplaint = true;
                    message.IsComplaintAccepted = true;
                    message.IsComplaintFalse = false;
                    break;
                case "false":
                    message.IsComplaint = true;
                    message.IsComplaintAccepted = false;
                    message.IsComplaintFalse = true;
                    message.IsComplaintResolved = false;
                    break;
                case "resolve":
                    message.IsComplaint = true;
                    message.IsComplaintAccepted = true;
                    message.IsComplaintResolved = true;
                    message.IsComplaintFalse = false;
                    break;
                default:
                    return BadRequest("Geçersiz işlem.");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Complaints");
        }

        public async Task<IActionResult> Categories()
        {
            ViewBag.ActiveTab = "Categories";

            var categories = await _context.MessageCategories
                                            .Select(c => new AdminCategoryListViewModel
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                Description = c.Description,
                                                Icon = c.Icon,
                                                IsSystemProtected = c.IsSystemProtected,
                                                ColorTheme = (int)c.Theme,
                                                MessageCount = _context.UserMessages.Count(m => m.MessageCategoryId == c.Id)
                                            }).ToListAsync();

            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(MessageCategory messageCategory)
        {
            var newCategory = _context.MessageCategories.Add(messageCategory);
            _context.SaveChanges();

            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(AdminCategoryListViewModel model)
        {
            var category = await _context.MessageCategories.FindAsync(model.Id);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = model.Name;
            category.Description = model.Description;
            category.Icon = model.Icon;
            category.Theme = (CategoryTheme)model.ColorTheme;

            _context.MessageCategories.Update(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Categories");
        }

        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.MessageCategories.FindAsync(id);

            _context.MessageCategories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Categories");
        }
    }
}
