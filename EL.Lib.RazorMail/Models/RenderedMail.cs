using EL.Lib.RazorMail.Models;
using System.Collections.Generic;

namespace EL.Lib.RazorMail.Core
{
    /// <summary>
    /// This class gets returned from the isolated template after it has been
    /// rendered, and separates the various portions of the e-mail.
    /// </summary>
    public class RenderedMail
    {
        public RenderedMail(string? subject, string htmlBody, Dictionary<string, MailAttachment>? attachments)
        {
            Subject = subject;
            HtmlBody = htmlBody;
            Attachments = attachments;
        }

        public string? Subject { get;private set; }
        public string HtmlBody { get; private set; }
        public Dictionary<string, MailAttachment>? Attachments { get;private set; }
    }
}
