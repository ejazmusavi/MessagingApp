using MessagingApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MessagingApp.Helper;

namespace MessagingApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private const string EncKey = "b14ca5898a4e4133bbce2ea2315a1916";
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles ="Admin")]
        public IActionResult MessageList()
        {
            var list = _context.Messages.Include(i=>i.Useer).ToList();
            return View(list);
        }

        public IActionResult CreateMessage()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateMessage(Message model)
        {
            if (ModelState.IsValid)
            {
                string pwd = GeneratePassword();
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Message messag = new Message();
                messag.MessagDescription = model.MessagDescription;
                messag.CreatedBy = userId;
                messag.CreatedOn = DateTime.Now;
                messag.Password = SecurityHelper.EncryptString(EncKey, pwd);
                await _context.AddAsync(messag);
                await _context.SaveChangesAsync();

                var callbackUrl = Url.Action(
                        "ReadMessage",
                        "Home",
                        values: new { area = "", id =messag.Id
                        //System.Web.HttpUtility.UrlEncode(SecurityHelper.EncryptString(EncKey, messag.Id.ToString())) 
                        },
                        protocol: Request.Scheme);

                ViewBag.link = callbackUrl;
                ViewBag.password = pwd;
                return View();
            }
            else
            {
                return View(model);
            }

        }

        [AllowAnonymous]
        public IActionResult ReadMessage(int id)
        {
            ViewBag.id = id;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult Read(int id, string password)
        {
            //var _id = SecurityHelper.DecryptString(EncKey, System.Web.HttpUtility.UrlDecode( id));
            //if (int.TryParse(_id, out int pk))
            {
                var message = _context.Messages.Where(w=>w.Id==id).Select(s=>new { s.Password,s.MessagDescription,s.Id }).FirstOrDefault();
                if (message == null)
                {
                    return Json(new { status = false, message = "The message url is invalid." });
                }
                if (!string.IsNullOrEmpty(password))
                {
                    string mpassword = SecurityHelper.DecryptString(EncKey, message.Password);
                    if (mpassword.Equals(password))
                    {
                        return Json(new { status = true, message = new { message.Id, text = message.MessagDescription } });
                    }
                    else
                    {
                        return Json(new { status = false, message = "Provided password is invalid." });
                    }
                }
                return Json(new { status = false, message = "Provided password is invalid." });
            }
            //return Json(new { status = false, message = "The message url is invalid." });

        }
        private string GeneratePassword()
        {
            bool digit = true, lowercase = true, uppercase = true, nonalphanumeric = true;
            StringBuilder password=new StringBuilder();
            Random random = new Random();
            while (password.Length < 6)
            {
                char c = (char)random.Next(32, 126);
                password.Append(c);
                if (char.IsDigit(c))
                    digit = false;
                else if (char.IsLower(c))
                    lowercase = false;
                else if (char.IsUpper(c))
                    uppercase = false;
                else if (char.IsLetterOrDigit(c))
                    nonalphanumeric = false;
            }
            if (nonalphanumeric)
                password.Append((char)random.Next(33, 48));
            if (digit)
                password.Append((char)random.Next(48, 50));
            if (lowercase)
                password.Append((char)random.Next(97, 123));
            if (uppercase)
                password.Append((char)random.Next(65, 91));

            return password.ToString();
        }
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var exist = _context.Messages.Find(id);
            if (exist != null)
            {
                return View(exist);
            }
            else
            {
                return NotFound();
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(Message model)
        {
            var msgToUpdate = _context.Messages.Find(model.Id);
            if (msgToUpdate != null)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                msgToUpdate.MessagDescription = model.MessagDescription;
                msgToUpdate.ModifiedOn = DateTime.Now;
                msgToUpdate.ModifiedBy = userId;
                _context.Update(msgToUpdate);
                await _context.SaveChangesAsync();
                return View();
            }
            else
            {
                return View(model);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var msgtoDel = _context.Messages.Find(id);
            if (msgtoDel != null)
            {
                _context.Remove(msgtoDel);
                _context.SaveChanges();
                return Json(new { status = true, message = "Selected records has been deleted." });
            }
            else
            {
                return Json(new { status = false, message = "An error occured while deleting the record!!" });
            }
        }

    }
}
