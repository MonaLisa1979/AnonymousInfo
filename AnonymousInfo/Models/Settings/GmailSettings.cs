namespace AnonymousInfo.Models
{
    public class GmailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public SmtpSettings SMTP { get; set; }
    }
}
