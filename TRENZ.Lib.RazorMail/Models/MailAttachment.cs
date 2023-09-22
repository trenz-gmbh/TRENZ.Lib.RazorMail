using Microsoft.Extensions.FileSystemGlobbing.Internal;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EL.Lib.RazorMail.Models
{
    public class MailAttachment
    {
        public byte[] FileData { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; }

        public MailAttachment(byte[] fileData, string filename, string contentType)
        {
            FileData = fileData;
            Filename = filename;
            ContentType = contentType;
        }

        public string? ContentId { get; set; }
        public bool Inline { get; set; }

        public static implicit operator System.Net.Mail.Attachment(MailAttachment attachment)
        {
            var result = new System.Net.Mail.Attachment(new MemoryStream(attachment.FileData), attachment.Filename, attachment.ContentType);

            result.ContentId = attachment.ContentId;
            result.ContentDisposition.Inline = attachment.Inline;

            return result;
        }

        public static implicit operator MimePart(MailAttachment attachment)
        {
            var mimeContent = new MimeContent(new MemoryStream(attachment.FileData));
            var contentDisposition = 
                new ContentDisposition(attachment.Inline ? ContentDisposition.Inline : ContentDisposition.Attachment);

            var result = new MimePart(MimeKit.ContentType.Parse(attachment.ContentType))
            {
                ContentId = attachment.ContentId,
                Content = mimeContent,
                ContentDisposition = contentDisposition,
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = attachment.Filename
            };

            return result;
        }
    }
}
