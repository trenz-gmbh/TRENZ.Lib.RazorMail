using System.Text;

using JetBrains.Annotations;

using RazorMailMessage = TRENZ.Lib.RazorMail.Models.MailMessage;
using SystemNetMailMessage = System.Net.Mail.MailMessage;

namespace TRENZ.Lib.RazorMail.Extensions;

/// <summary>
/// Extension methods for <see cref="RazorMailMessage"/> to convert it to <see cref="SystemNetMailMessage"/>.
/// </summary>
public static class SystemNetMailMessageExtensions
{
    /// <summary>
    /// Converts a <see cref="RazorMailMessage"/> to a <see cref="SystemNetMailMessage"/>.
    /// </summary>
    /// <param name="razorMessage">The <see cref="RazorMailMessage"/> to convert.</param>
    /// <returns>The converted <see cref="SystemNetMailMessage"/>.</returns>
    [MustDisposeResource]
    public static SystemNetMailMessage ToSystemNetMailMessage(this RazorMailMessage razorMessage)
    {
        var systemNetMessage = new SystemNetMailMessage();

        systemNetMessage.BodyEncoding = Encoding.UTF8;
        systemNetMessage.IsBodyHtml = true;

        SetMailHeaders(razorMessage, systemNetMessage);
        SetMailContent(razorMessage, systemNetMessage);

        return systemNetMessage;
    }

    private static void SetMailHeaders(RazorMailMessage razorMessage, SystemNetMailMessage systemNetMessage)
    {
        systemNetMessage.From = razorMessage.Headers.From!.ToMailAddress();

        foreach (var item in razorMessage.Headers.Recipients)
            systemNetMessage.To.Add(item.ToMailAddress());

        foreach (var item in razorMessage.Headers.CarbonCopy)
            systemNetMessage.CC.Add(item.ToMailAddress());

        foreach (var item in razorMessage.Headers.BlindCarbonCopy)
            systemNetMessage.Bcc.Add(item.ToMailAddress());

        foreach (var item in razorMessage.Headers.ReplyTo)
            systemNetMessage.ReplyToList.Add(item.ToMailAddress());

        foreach (var (name, value) in razorMessage.Headers.NonAddressHeaders)
        {
            systemNetMessage.Headers.Add(name, value.ToString());
        }
    }

    private static void SetMailContent(RazorMailMessage razorMessage, SystemNetMailMessage systemNetMessage)
    {
        systemNetMessage.Subject = razorMessage.Content.Subject;
        systemNetMessage.Body = razorMessage.Content.HtmlBody;

        foreach (var item in razorMessage.Content.Attachments.Values)
            systemNetMessage.Attachments.Add(item.ToAttachment());
    }
}
