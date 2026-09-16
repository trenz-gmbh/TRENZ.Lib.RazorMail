using Microsoft.Graph.Models;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MicrosoftGraph.Extensions;

public static class MailAttachmentExtensions
{
    public static FileAttachment ToMsFileAttachment(this MailAttachment mailAttachment)
    {
        var attachment = new FileAttachment
        {
            ContentBytes = mailAttachment.FileData,
            Name = mailAttachment.FileName,
            ContentType = mailAttachment.ContentType,
            ContentId = mailAttachment.ContentId,
            IsInline = mailAttachment.Inline
        };
        return attachment;
    }
}
