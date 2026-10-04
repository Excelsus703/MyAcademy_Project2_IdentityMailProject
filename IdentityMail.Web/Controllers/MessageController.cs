using IdentityMail.Web.Context;
using IdentityMail.Web.DTOs.UserMessageDtos;
using IdentityMail.Web.Entites;
using IdentityMail.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading.Tasks;

namespace IdentityMail.Web.Controllers
{
    [Authorize]
    public class MessageController(UserManager<AppUser> _userManager,
                                    AppDbContext _context) : Controller
    {
        public async Task<IActionResult> Index(int? id, string sortBy = "date_desc")
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.fullName = user.FirstName + " " + user.LastName;

            var query = _context.UserMessages.Include(x => x.Sender).Where(x => x.ReceiverId == user.Id && x.isTrash == false && x.isDraft == false);

            query = sortBy switch
            {
                "date_asc" => query.OrderBy(m => m.SendDate),
                "subject_asc" => query.OrderBy(m => m.Subject),
                "subject_desc" => query.OrderByDescending(m => m.Subject),
                "read" => query.Where(m => m.isRead == true).OrderByDescending(m => m.SendDate),
                "unread" => query.Where(m => m.isRead == false).OrderByDescending(m => m.SendDate),
                "important" => query.Where(m => m.isImportant == true).OrderByDescending(m => m.SendDate),
                _ => query.OrderByDescending(m => m.SendDate)
            };

            var messages = await query.ToListAsync();

            ViewBag.CurrentSort = sortBy;
            ViewBag.ActiveTab = "Index";
            ViewBag.SelectedId = id;

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsComplaint(int id, string complaintReason)
        {
            var message = await _context.UserMessages.FindAsync(id);
            message.IsComplaint = true;
            message.ComplaintReason = complaintReason;

            _context.UserMessages.Update(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Message", new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> QuickSearch(MessageSearchFilterViewModel messageSearchFilterViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(messageSearchFilterViewModel.SearchTerm))
            {
                return Json(new List<QuickSearchResultViewModel>());
            }

            var term = messageSearchFilterViewModel.SearchTerm.ToLower().Trim();
            var searchType = messageSearchFilterViewModel.SearchType?.ToLower() ?? "all";

            var query = _context.UserMessages.Include(x => x.Sender)
                                             .Where(x => x.ReceiverId == user.Id && x.isTrash == false && x.isDraft == false)
                                             .AsQueryable();

            switch (searchType)
            {
                case "sender":
                    query = query.Where(m => m.Sender.FirstName.ToLower().Contains(term) ||
                                             m.Sender.LastName.ToLower().Contains(term));
                    break;
                case "subject":
                    query = query.Where(m => m.Subject.ToLower().Contains(term));
                    break;
                case "body":
                    query = query.Where(m => m.Body.ToLower().Contains(term));
                    break;
                default:
                    query = query.Where(m => m.Subject.ToLower().Contains(term) ||
                                             m.Sender.FirstName.ToLower().Contains(term) ||
                                             m.Sender.LastName.ToLower().Contains(term) ||
                                             m.Body.ToLower().Contains(term));
                    break;
            }

            if (messageSearchFilterViewModel.CategoryId.HasValue)
            {
                query = query.Where(m => m.MessageCategoryId == messageSearchFilterViewModel.CategoryId.Value);
            }

            if (messageSearchFilterViewModel.StartDate.HasValue)
            {
                query = query.Where(m => m.SendDate >= messageSearchFilterViewModel.StartDate.Value);
            }

            if (messageSearchFilterViewModel.EndDate.HasValue)
            {
                query = query.Where(m => m.SendDate <= messageSearchFilterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));// gün sonuna kadar
            }

            var results = await query.OrderByDescending(m => m.SendDate)
                                     .Take(5)
                                     .Select(m => new QuickSearchResultViewModel
                                     {
                                         Id = m.Id,
                                         SenderFullName = m.Sender.FirstName + " " + m.Sender.LastName,
                                         Subject = m.Subject.Length > 35 ? m.Subject.Substring(0, 35) + "..." : m.Subject,
                                         DateStr = m.SendDate.ToString("dd MMM")
                                     }).ToListAsync();

            return Json(results);
        }

        [HttpGet]
        public async Task<IActionResult> SendMail()
        {
            var categories = await _context.MessageCategories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMail(SendMailDto sendMailDto)
        {
            if (string.IsNullOrWhiteSpace(sendMailDto.ReceiverMail))
            {
                ViewBag.Categories = new SelectList(await _context.MessageCategories.ToListAsync(), "Id", "Name");

                return View(sendMailDto);
            }

            var sender = await _userManager.FindByNameAsync(User.Identity.Name);
            var receiver = await _userManager.FindByEmailAsync(sendMailDto.ReceiverMail);

            if (receiver is null)
            {
                ModelState.AddModelError(string.Empty, "Girdiğiniz Mail ile sistemde kayıtlı kullanıcı bulunamadı.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _context.MessageCategories.ToListAsync(), "Id", "Name");

                return View(sendMailDto);
            }

            var newMessage = new UserMessage
            {
                SendDate = DateTime.Now,
                ReceiverId = receiver.Id,
                SenderId = sender.Id,
                Subject = sendMailDto.Subject,
                Body = sendMailDto.Body,
                MessageCategoryId = sendMailDto.MessageCategoryId,
                isDraft = false
            };


            if (sendMailDto.Attachments != null && sendMailDto.Attachments.Count > 0)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/attachments");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                foreach (var file in sendMailDto.Attachments)
                {
                    if (file.Length > 0)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        var savedName = $"{Guid.NewGuid()}{extension}";
                        var fullPath = Path.Combine(uploadFolder, savedName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        newMessage.MessageAttachments.Add(new MessageAttachment
                        {
                            OriginalName = file.FileName,
                            SavedName = savedName,
                            FilePath = "/uploads/attachments/" + savedName,
                            FileType = extension,
                            FileSize = file.Length
                        });
                    }
                }
            }


            _context.UserMessages.Add(newMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");

        }

        [HttpPost]
        public async Task<IActionResult> ToggleImportant(int id)
        {
            var message = await _context.UserMessages.FindAsync(id);

            if (message != null)
            {
                message.isImportant = !message.isImportant;

                _context.UserMessages.Update(message);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> MoveToTrash(int id)
        {
            var message = await _context.UserMessages.FindAsync(id);

            if (message != null)
            {
                message.isTrash = true;
                _context.UserMessages.Update(message);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> TrashBox(int? id)
        {
            var trashedMessage = await _context.UserMessages.Include(x => x.Sender)
                                                            .Where(x => x.isTrash == true)
                                                            .OrderByDescending(x => x.SendDate)
                                                            .ToListAsync();
            ViewBag.ActiveTab = "TrashBox";
            ViewBag.SelectedMessageId = id;

            return View(trashedMessage);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreFromTrash(int id)
        {
            var message = await _context.UserMessages.FindAsync(id);

            if (message != null)
            {
                message.isTrash = false;
                _context.UserMessages.Update(message);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("TrashBox");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePermanently(int id)
        {
            var message = await _context.UserMessages.FindAsync(id);

            if (message != null)
            {
                _context.UserMessages.Remove(message);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("TrashBox");
        }

        [HttpGet]
        public async Task<IActionResult> ImportantMessages(int? id, string sortBy = "date_desc")
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.fullName = user.FirstName + " " + user.LastName;

            //var messages = await _context.UserMessages.Include(x => x.Sender).Where(x => x.ReceiverId == user.Id && x.isTrash == false && x.isDraft == false && x.isImportant == true).OrderByDescending(x => x.SendDate).ToListAsync();

            var query = _context.UserMessages.Include(x => x.Sender).Where(x => x.ReceiverId == user.Id && x.isTrash == false && x.isDraft == false && x.isImportant == true);

            query = sortBy switch
            {
                "date_asc" => query.OrderBy(m => m.SendDate),
                "subject_asc" => query.OrderBy(m => m.Subject),
                "subject_desc" => query.OrderByDescending(m => m.Subject),
                _ => query.OrderByDescending(m => m.SendDate)
            };

            var messages = await query.ToListAsync();

            ViewBag.CurrentSort = sortBy;
            ViewBag.ActiveTab = "ImportantMessages";
            ViewBag.SelectedId = id;

            return View(messages);
        }

        [HttpGet]
        public async Task<IActionResult> SentMessages(int? id)
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.fullName = user.FirstName + " " + user.LastName;

            var messages = await _context.UserMessages.Include(x => x.Sender).Where(x => x.SenderId == user.Id && x.isTrash == false && x.isDraft == false).OrderByDescending(x => x.SendDate).ToListAsync();

            ViewBag.ActiveTab = "SentMessages";
            ViewBag.SelectedId = id;

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SaveDraft(SendMailDto sendMailDto)
        {
            var user = await _userManager.GetUserAsync(User);

            ModelState.Remove("ReceiverMail");

            int receiverIfNullorNot = user.Id;
            if (!string.IsNullOrWhiteSpace(sendMailDto.ReceiverMail))
            {
                var receiver = await _userManager.FindByEmailAsync(sendMailDto.ReceiverMail);
                if (receiver != null)
                {
                    receiverIfNullorNot = receiver.Id;
                }
            }

            var newMessage = new UserMessage
            {
                SendDate = DateTime.Now,
                ReceiverId = receiverIfNullorNot,
                SenderId = user.Id,
                Subject = sendMailDto.Subject ?? "Konusuz Taslak",
                Body = sendMailDto.Body ?? "Mesaj Yazılmamış Taslak",
                isDraft = true
            };

            if (sendMailDto.Attachments != null && sendMailDto.Attachments.Count > 0)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/attachments");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                foreach (var file in sendMailDto.Attachments)
                {
                    if (file.Length > 0)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        var savedName = $"{Guid.NewGuid()}{extension}";
                        var fullPath = Path.Combine(uploadFolder, savedName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        newMessage.MessageAttachments.Add(new MessageAttachment
                        {
                            OriginalName = file.FileName,
                            SavedName = savedName,
                            FilePath = "/uploads/attachments/" + savedName,
                            FileType = extension,
                            FileSize = file.Length
                        });
                    }
                }
            }

            _context.UserMessages.Add(newMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("DraftMessages");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDraft(SendMailDto sendMailDto, int id)
        {
            var user = await _userManager.GetUserAsync(User);

            ModelState.Remove("ReceiverMail");

            var draft = await _context.UserMessages
                .Include(m => m.MessageAttachments)
                .FirstOrDefaultAsync(m => m.Id == id && m.SenderId == user.Id && m.isDraft == true);

            if (draft == null)
            {
                return NotFound();
            }

            int receiverIfNullorNot = user.Id;
            if (!string.IsNullOrWhiteSpace(sendMailDto.ReceiverMail))
            {
                var receiver = await _userManager.FindByEmailAsync(sendMailDto.ReceiverMail);
                if (receiver != null)
                {
                    receiverIfNullorNot = receiver.Id;
                }
            }

            draft.SendDate = DateTime.Now;
            draft.ReceiverId = receiverIfNullorNot;
            draft.Subject = string.IsNullOrWhiteSpace(sendMailDto.Subject) ? "Konusuz Taslak" : sendMailDto.Subject;
            draft.Body = string.IsNullOrWhiteSpace(sendMailDto.Body) ? "Mesaj Yazılmamış Taslak" : sendMailDto.Body;
            draft.MessageCategoryId = sendMailDto.MessageCategoryId;

            if (sendMailDto.Attachments != null && sendMailDto.Attachments.Count > 0)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/attachments");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                foreach (var file in sendMailDto.Attachments)
                {
                    if (file.Length > 0)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        var savedName = $"{Guid.NewGuid()}{extension}";
                        var fullPath = Path.Combine(uploadFolder, savedName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        draft.MessageAttachments.Add(new MessageAttachment
                        {
                            OriginalName = file.FileName,
                            SavedName = savedName,
                            FilePath = "/uploads/attachments/" + savedName,
                            FileType = extension,
                            FileSize = file.Length
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("DraftMessages");
        }

        [HttpGet]
        public async Task<IActionResult> DraftMessages(int? id)
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.fullName = user.FirstName + " " + user.LastName;

            var messages = await _context.UserMessages.Include(x => x.Receiver).Where(x => x.SenderId == user.Id && x.isTrash == false && x.isDraft == true).OrderByDescending(x => x.SendDate).ToListAsync();

            ViewBag.ActiveTab = "DraftMessages";
            ViewBag.SelectedId = id;

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDraft(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var draft = await _context.UserMessages.FirstOrDefaultAsync(x => x.Id == id && x.SenderId == user.Id && x.isDraft == true);

            if (draft != null)
            {
                _context.UserMessages.Remove(draft);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("DraftMessages");
        }
    }
}
