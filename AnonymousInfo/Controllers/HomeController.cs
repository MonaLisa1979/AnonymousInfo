using AnonymousInfo.Models;
using AnonymousInfo.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;

namespace AnonymousInfo.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
        private readonly GmailSettings gmailSettings;
        private readonly IMailService mailService;
        private readonly IWebHostEnvironment webHostEnvironment;

        public HomeController(IOptions<GmailSettings> gmailSettings, IWebHostEnvironment webHostEnvironment, IMailService mailService)
        {
            this.gmailSettings = gmailSettings.Value;
            this.webHostEnvironment = webHostEnvironment;
            this.mailService = mailService;
        }

        [Route("")]
        [Route("~/")]
        [Route("index")]
        public IActionResult Index()
        {
            return View("Index", new Contact());
        }

        [HttpPost]
        [Route("send")]
        public IActionResult Send(Contact contact, IFormFile[] attachments)
        {
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
            return View("Index", new Contact());
        }
    }
}
