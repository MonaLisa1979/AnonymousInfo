using AnonymousInfo.Models;
using AnonymousInfo.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AnonymousInfo.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
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
                ViewBag.msg = "Fields are empty";
                return View("Index", contact);
            }

            // validate input
            var requestIsValid = await this.verificationService.IsCaptchaValid(captcha.Token);

            if (!requestIsValid)
            {
                ViewBag.msg = "Capcha is not valid";
                return View("Index", contact);
            }

            Dictionary<string, Stream> files = new();

            if (attachments != null && attachments.Length > 0)
            {
                foreach (IFormFile attachment in attachments)
                {
                    var filename = Path.GetFileName(attachment.FileName);
                    var stream = attachment.OpenReadStream();
                    files[filename] = stream;
                }
            }

            var email = this.gmailSettings.Username;

            if (this.mailService.Send(email, email, contact.Subject, contact.Content, files))
            {
                ViewBag.msg = "Sent Mail Successfully";
            }
            else
            {
                ViewBag.msg = "Failed";
            }
            return View("Index", new InfoFormViewModel());
        }
    }
}
