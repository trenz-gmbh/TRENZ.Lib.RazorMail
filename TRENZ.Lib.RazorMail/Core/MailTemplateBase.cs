using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;

using MimeKit;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Core;

public abstract class MailTemplateBase<T> : RazorPage<T>
{
    public Dictionary<string, MailAttachment> Attachments
    {
        get
        {
            if (!this.ViewData.TryGetValue("Attachments", out var dict))
            {
                dict = new Dictionary<string, MailAttachment>();

                this.ViewData["Attachments"] = dict;
            }

            return (Dictionary<string, MailAttachment>)dict;
        }
    }

    public string? Subject
    {
        get => this.ViewData["Subject"] as string;
        set => this.ViewData["Subject"] = value;
    }

    public string InlineFile(string filename)
        => _AttachFile(filename, new ContentDisposition(ContentDisposition.Inline));

    public void AttachFile(string filename)
        => _AttachFile(filename, new ContentDisposition(ContentDisposition.Attachment));

    public string InlineFile(string filename, byte[] fileData)
        => _AttachFile(filename, fileData, new ContentDisposition(ContentDisposition.Inline));

    public void AttachFile(string filename, byte[] fileData)
        => _AttachFile(filename, fileData, new ContentDisposition(ContentDisposition.Attachment));

    private string _AttachFile(string filename, ContentDisposition contentDisposition)
    {
        var contentRootPath = TempData["ContentRootPath"] as string ??
                              throw new InvalidOperationException(nameof(TempData));

        var viewPath = this.Path.TrimStart('/');

        var parentDir = System.IO.Path.GetDirectoryName(viewPath) ?? throw new ArgumentException(nameof(this.Path));

        var absolutePath = System.IO.Path.Combine(contentRootPath, parentDir, filename);

        return _AttachFile(filename, System.IO.File.ReadAllBytes(absolutePath), contentDisposition);
    }

    private string _AttachFile(string filename, byte[] fileData, ContentDisposition contentDisposition)
    {
        if (!new FileExtensionContentTypeProvider().TryGetContentType(filename, out var contentType))
            throw new NotSupportedException($"Couldn't figure out content type for {filename}");

        if (!Attachments.ContainsKey(filename))
        {
            var attachment = new MailAttachment(fileData, filename, contentType);

            switch (contentDisposition.Disposition)
            {
                case ContentDisposition.Inline:
                    attachment.ContentId = filename;
                    attachment.Inline = true;

                    break;
                case ContentDisposition.Attachment:
                    attachment.Inline = false;

                    break;
                default:
                    break;
            }

            Attachments[filename] = attachment;
        }

        return $"cid:{filename}";
    }
}