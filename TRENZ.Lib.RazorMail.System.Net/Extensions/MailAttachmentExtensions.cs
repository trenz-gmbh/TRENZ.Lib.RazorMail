using System.IO;

using JetBrains.Annotations;

using RazorMailAttachment = TRENZ.Lib.RazorMail.Models.MailAttachment;
using SystemNetMailAttachment = System.Net.Mail.Attachment;

namespace TRENZ.Lib.RazorMail.Extensions;

public static class MailAttachmentExtensions
{
    [MustDisposeResource]
    public static SystemNetMailAttachment ToAttachment(this RazorMailAttachment attachment)
    {
        // Stream is disposed by the Attachment class
        var stream = new MemoryStream(attachment.FileData);
        var result = new SystemNetMailAttachment(stream, attachment.FileName, attachment.ContentType);

        result.ContentId = attachment.ContentId;
        result.ContentDisposition!.Inline = attachment.Inline;

        return result;
    }
}
