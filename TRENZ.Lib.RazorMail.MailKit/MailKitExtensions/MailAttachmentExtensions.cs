using System.IO;

using MimeKit;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MailKitExtensions;

public static class MailAttachmentExtensions
{
    public static MimePart ToMimePart(this MailAttachment attachment)
    {
        var mimeContent = new MimeContent(new MemoryStream(attachment.FileData));
        var contentDisposition =
            attachment.Inline ? MimeKit.ContentDisposition.Inline : MimeKit.ContentDisposition.Attachment;

        var result = new MimePart(MimeKit.ContentType.Parse(attachment.ContentType))
        {
            ContentId = attachment.ContentId,
            Content = mimeContent,
            ContentDisposition = new MimeKit.ContentDisposition(contentDisposition),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = attachment.Filename
        };

        return result;
    }
}