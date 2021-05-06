using System.ComponentModel.DataAnnotations;

namespace AnonymousInfo.Models
{
    public class InfoFormViewModel : CaptchaSettings
    {
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
    