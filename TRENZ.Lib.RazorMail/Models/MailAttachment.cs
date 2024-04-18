using System.IO;

namespace TRENZ.Lib.RazorMail.Models;

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
}