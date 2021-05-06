using AnonymousInfo.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AnonymousInfo.Services
{
    public class CaptchaVerificationService
    {
        private CaptchaSettings captchaSettings;
        private ILogger<CaptchaVerificationService> logger;

        public string ClientKey => captchaSettings.ClientKey;

        public CaptchaVerificationService(IOptions<CaptchaSettings> captchaSettings, ILogger<CaptchaVerificationService> logger)
        {
            this.captchaSettings = captchaSettings.Value;
            this.logger = logger;
        }

        public async Task<bool> IsCaptchaValid(string token)
        {
            var result = false;

            try
            {
                using var client = new HttpClient();

                var response = await client.PostAsync($"{captchaSettings.GoogleVerificationUrl}?secret={captchaSettings.ServerKey}&response={token}", null);
                var jsonString = await response.Content.ReadAsStringAsync();
                var captchaVerfication = JsonConvert.DeserializeObject<CaptchaVerificationResponse>(jsonString);

                result = captchaVerfication.Success;
            }
            catch (Exception exc)
            {
                logger.LogError("Failed to process captcha validation", exc);
            }

            return result;
        }
    }
}
