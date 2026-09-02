using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Tests;

/// <summary>
/// Builds throwaway <see cref="MailMessage"/> instances that satisfy the validation performed by
/// <see cref="Services.BaseSmtpMailClient.SendAsync"/>, so tests only have to spell out the part
/// they actually care about.
/// </summary>
internal static class TestMessages
{
    /// <summary>
    /// Creates a header collection with a sender and a single recipient.
    /// </summary>
    public static MailHeaderCollection CreateValidHeaders()
    {
        var headers = new MailHeaderCollection
        {
            From = new MailAddress("sender@example.test", "Sender"),
        };

        headers.AddRecipient(new MailAddress("recipient@example.test", "Recipient"));

        return headers;
    }

    /// <summary>
    /// Wraps the given headers in a message with a non-empty HTML body.
    /// </summary>
    public static MailMessage Create(MailHeaderCollection headers) => new()
    {
        Headers = headers,
        Content = new MailContent
        {
            Subject = "Subject",
            HtmlBody = "<p>Hello</p>",
        },
    };

    /// <summary>
    /// Creates a message that passes validation: a sender, one recipient, and an HTML body.
    /// </summary>
    public static MailMessage CreateValid() => Create(CreateValidHeaders());
}
