using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Core;

public abstract class MailTemplateBase<T> : RazorPage<T>
{
    internal const string AttachmentsKey = "Attachments";
    internal const string SubjectKey = "Subject";
    internal const string ContentRootPathKey = "ContentRootPath";

    public Dictionary<string, MailAttachment> Attachments
    {
        get
        {
            if (ViewData.TryGetValue(AttachmentsKey, out var dict))
                return (Dictionary<string, MailAttachment>)dict!;

            dict = new Dictionary<string, MailAttachment>();

            ViewData[AttachmentsKey] = dict;

            return (Dictionary<string, MailAttachment>)dict;
        }
    }

    public string? Subject
    {
        get => ViewData[SubjectKey] as string;
        set => ViewData[SubjectKey] = value;
    }

    public string InlineFile(string filename)
        => _AttachFile(filename, ContentDisposition.Inline);

    public void AttachFile(string filename)
        => _AttachFile(filename, ContentDisposition.Attachment);

    public string InlineFile(string filename, byte[] fileData)
        => _AttachFile(filename, fileData, ContentDisposition.Inline);

    public void AttachFile(string filename, byte[] fileData)
        => _AttachFile(filename, fileData, ContentDisposition.Attachment);

    private string _AttachFile(string fileName, ContentDisposition contentDisposition)
    {
        var contentRootPath = TempData["ContentRootPath"] as string ??
                              throw new InvalidOperationException(nameof(TempData));

        var viewPath = Path.TrimStart('/');

        var parentDir = System.IO.Path.GetDirectoryName(viewPath) ?? throw new ArgumentException(nameof(this.Path));

        var absolutePath = System.IO.Path.Combine(contentRootPath, parentDir, fileName);

        return _AttachFile(fileName, File.ReadAllBytes(absolutePath), contentDisposition);
    }

    private string _AttachFile(string fileName, byte[] fileData, ContentDisposition contentDisposition)
    {
        if (!new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType))
            throw new NotSupportedException($"Couldn't figure out content type for {fileName}");

        var cid = $"cid:{fileName}";
        if (Attachments.ContainsKey(fileName))
            return cid;

        var attachment = new MailAttachment
        {
            FileName = fileName,
            FileData = fileData,
            ContentType = contentType,
        };

        switch (contentDisposition)
        {
            case ContentDisposition.Inline:
                attachment.ContentId = fileName;
                attachment.Inline = true;

                break;
            case ContentDisposition.Attachment:
                attachment.Inline = false;

                break;
            case ContentDisposition.Unset:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(contentDisposition), contentDisposition, "Unknown content disposition");
        }

        Attachments[fileName] = attachment;

        return cid;
    }
}
