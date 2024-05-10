namespace TRENZ.Lib.RazorMail.Models;

/// <summary>
/// A mail attachment.
/// </summary>
public class MailAttachment
{
    /// <summary>
    /// The data of the file.
    /// </summary>
    public required byte[] FileData { get; set; }

    /// <summary>
    /// The name of the file.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// The content type of the file.
    /// </summary>
    public required string ContentType { get; set; }

    /// <summary>
    /// The content ID of the attachment.
    /// </summary>
    public string? ContentId { get; set; }

    /// <summary>
    /// Whether the attachment should be displayed inline.
    /// </summary>
    public bool Inline { get; set; }
}
