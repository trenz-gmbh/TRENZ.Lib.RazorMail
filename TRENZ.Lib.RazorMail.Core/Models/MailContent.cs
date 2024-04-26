using System.Collections.Generic;

namespace TRENZ.Lib.RazorMail.Models;

/// <summary>
/// This class gets returned from the isolated template after it has been
/// rendered, and separates the various portions of the e-mail.
/// </summary>
public class MailContent
{
    /// <summary>
    /// The subject of the mail.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// The HTML body of the mail.
    /// </summary>
    public required string HtmlBody { get; set; }

    /// <summary>
    /// Attachments for the mail.
    /// </summary>
    public IDictionary<string, MailAttachment> Attachments { get; init; } = new Dictionary<string, MailAttachment>();
}
