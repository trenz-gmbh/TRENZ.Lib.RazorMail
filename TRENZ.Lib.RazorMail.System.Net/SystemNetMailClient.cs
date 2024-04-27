using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TRENZ.Lib.RazorMail.Extensions;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.Services;

using RazorMailMessage = TRENZ.Lib.RazorMail.Models.MailMessage;
using SystemNetMailMessage = System.Net.Mail.MailMessage;

namespace TRENZ.Lib.RazorMail;

public class SystemNetMailClient(IOptions<SmtpAccount> accountOptions, ILogger<SystemNetMailClient> logger) : BaseMailClient(accountOptions)
{
    [MustDisposeResource]
    private SmtpClient CreateClient()
    {
        var client = new SmtpClient(Account.Host);

        client.Port = Account.Port;
        client.EnableSsl = Account.TLS;
        client.Credentials = new NetworkCredential(Account.Login, Account.Password);

        return client;
    }

    /// <inheritdoc />
    protected override async Task SendInternalAsync(RazorMailMessage message, CancellationToken cancellationToken)
    {
        using var nativeMessage = ConvertToNative(message);
        using var client = CreateClient();

        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation("Sending mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc}) with subject {Subject}",
            nativeMessage.From, nativeMessage.To, nativeMessage.CC, nativeMessage.Bcc, nativeMessage.Subject);

        await client.SendAsyncWithCancellation(nativeMessage, cancellationToken);
    }

    [MustDisposeResource]
    private static SystemNetMailMessage ConvertToNative(RazorMailMessage razorMessage)
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
    }

    private static void SetMailContent(RazorMailMessage razorMessage, SystemNetMailMessage systemNetMessage)
    {
        systemNetMessage.Subject = razorMessage.Content.Subject;
        systemNetMessage.Body = razorMessage.Content.HtmlBody;

        foreach (var item in razorMessage.Content.Attachments.Values)
            systemNetMessage.Attachments.Add(item.ToAttachment());
    }
}
