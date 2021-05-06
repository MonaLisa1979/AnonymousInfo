using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnonymousInfo.Models
{
    public class CaptchaResponse
    {
        [BindProperty(Name = "g-recaptcha-response")]
        public string Token { get; set; }
    }
}
