using AnonymousInfo.Models;
using AnonymousInfo.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AnonymousInfo.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
        private const long fileSizeLimit = 25 * 1024 * 1024;
        private readonly GmailSettings gmailSettings;
        private readonly CaptchaSettings captchaSettings;
        private readonly IMailService mailService;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly CaptchaVerificationService verificationService;

        public HomeController(
            IOptions<GmailSettings> gmailSettings,
            IOptions<CaptchaSettings> captchaSettings,
            IWebHostEnvironment webHostEnvironment, 
            IMailService mailService, 
            CaptchaVerificationService verificationService)
        {
            this.gmailSettings = gmailSettings.Value;
            this.captchaSettings = captchaSettings.Value;
            this.webHostEnvironment = webHostEnvironment;
            this.mailService = mailService;
            this.verificationService = verificationService;
        }

        public string CaptchaClientKey { get; set; }

        [Route("")]
        [Route("~/")]
        [Route("index")]
        public IActionResult Index()
        {
            return View("Index", new InfoFormViewModel() { ClientKey = this.captchaSettings.ClientKey } );
        }

        [HttpPost]
        [Route("send")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(InfoFormViewModel contact, IFormFile[] attachments, CaptchaResponse captcha)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMsg = "Напиши мне что-нибудь. Плохо, когда поля пустые :-(";
                return View("Index", contact);
            }

            // validate input
            var requestIsValid = await this.verificationService.IsCaptchaValid(captcha.Token);

            if (!requestIsValid)
            {
                ViewBag.ErrorMsg = "Ты пытаешся нас сломать или это гугл приуныл?";
                return View("Index", contact);
            }

            Dictionary<string, Stream> files = new();

            if (attachments != null && attachments.Length > 0)
            {
                long totalSize = 0;
                foreach (IFormFile attachment in attachments)
                {
                    var filename = Path.GetFileName(attachment.FileName);
                    var stream = attachment.OpenReadStream();

                    totalSize += stream.Length;

                    if (totalSize > fileSizeLimit)
                    {
                        ViewBag.ErrorMsg = "Файл слишком большой";
                        return View("Index", contact);
                    }

                    files[filename] = stream;
                }
            }
                
            var email = this.gmailSettings.Username;

            if (this.mailService.Send(email, email, contact.Subject, contact.Content, files))
            {
                ModelState.Clear();
                ViewBag.SuccessMsg = "Вялікі дзякуй!";
                ViewBag.ErrorMsg = string.Empty;
            }
            else
            {
                ViewBag.ErrorMsg = "Упс.. Что то пошло не так. Мы уже разбираемся в этом. Попробуй чуть позже";
            }
            return View("Index", new InfoFormViewModel() { ClientKey = this.captchaSettings.ClientKey });
        }
    }
}
