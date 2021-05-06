using System.Collections.Generic;
using System.IO;

namespace AnonymousInfo.Services
{
    public interface IMailService
    {
        public bool Send(string from, string to, string subject, string content, Dictionary<string, Stream> attachments);
    }
}