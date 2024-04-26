using RazorMailAttachment = TRENZ.Lib.RazorMail.Models.MailAttachment;
using SystemNetMailAttachment = System.Net.Mail.Attachment;

namespace TRENZ.Lib.RazorMail.Extensions;

public static class MailAttachmentExtensions
{
    public static SystemNetMailAttachment ToAttachment(this RazorMailAttachment attachment)
    {
        var result = new SystemNetMailAttachment(attachment.FileStream, attachment.FileName, attachment.ContentType);

        result.ContentId = attachment.ContentId;
        result.ContentDisposition!.Inline = attachment.Inline;

        return result;
    }
}
