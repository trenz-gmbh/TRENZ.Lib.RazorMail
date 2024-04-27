using System.Linq;

using MimeKit;

namespace TRENZ.Lib.RazorMail.Extensions;

using RazorMailMessage = Models.MailMessage;
using MailKitMailMessage = MimeMessage;
using IMailKitMailMessage = IMimeMessage;

/// <summary>
/// Extension methods to convert a <see cref="RazorMailMessage"/> to a <see cref="MailKitMailMessage"/>.
/// </summary>
public static class MailMessageExtensions
{
    /// <summary>
    /// Converts a <see cref="RazorMailMessage"/> to a <see cref="MailKitMailMessage"/>.
    /// </summary>
    /// <param name="razorMessage">The <see cref="RazorMailMessage"/> to convert.</param>
    /// <returns>The converted <see cref="MailKitMailMessage"/>.</returns>
    public static MailKitMailMessage ToMimeMessage(this RazorMailMessage razorMessage)
    {
        var mailKitMessage = new MailKitMailMessage();

        SetMailHeaders(razorMessage, mailKitMessage);
        SetMailContent(razorMessage, mailKitMessage);

        return mailKitMessage;
    }

    private static void SetMailHeaders(RazorMailMessage razorMessage, IMailKitMailMessage mailMessage)
    {
        mailMessage.From.Add(razorMessage.Headers.From!.ToMailboxAddress());
        mailMessage.To.AddRange(razorMessage.Headers.Recipients.Select(x => x.ToMailboxAddress()));
        mailMessage.Cc.AddRange(razorMessage.Headers.CarbonCopy.Select(x => x.ToMailboxAddress()));
        mailMessage.Bcc.AddRange(razorMessage.Headers.BlindCarbonCopy.Select(x => x.ToMailboxAddress()));
        mailMessage.ReplyTo.AddRange(razorMessage.Headers.ReplyTo.Select(x => x.ToMailboxAddress()));
    }

    private static void SetMailContent(RazorMailMessage razorMessage, IMailKitMailMessage mailMessage)
    {
        mailMessage.Subject = razorMessage.Content.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = razorMessage.Content.HtmlBody,
        };

        foreach (var item in razorMessage.Content.Attachments.Values)
        {
            var attachment =
                bodyBuilder.Attachments.Add(item.FileName, item.FileData, ContentType.Parse(item.ContentType));
            attachment.ContentId = item.ContentId;
            attachment.IsAttachment = !item.Inline;
        }

        mailMessage.Body = bodyBuilder.ToMessageBody();
    }
}