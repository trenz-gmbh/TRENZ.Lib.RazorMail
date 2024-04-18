using System.IO;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.SystemNetExtensions;

public static class MailAttachmentExtensions
{
    public static System.Net.Mail.Attachment ToAttachment(this MailAttachment attachment)
    {
        var result = new System.Net.Mail.Attachment(new MemoryStream(attachment.FileData), attachment.Filename,
            attachment.ContentType);

        result.ContentId = attachment.ContentId;
        result.ContentDisposition.Inline = attachment.Inline;

        return result;
    }
}