namespace TRENZ.Lib.RazorMail.Models;

/// <summary>
/// Represents a mail message with headers and content
/// </summary>
public class MailMessage
{
    /// <summary>
    /// The headers of the mail message.
    /// </summary>
    public required MailHeaderCollection Headers { get; init; }

    /// <summary>
    /// The content of the mail message.
    /// </summary>
    public required MailContent Content { get; init; }
}
