using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Services;

/// <summary>
/// Creates a new mail sender with the specified SMTP account.
/// </summary>
/// <param name="account">The account to use for sending emails.</param>
public abstract class MailSender(SmtpAccount account)
{
    /// <summary>
    /// The SMTP account to use for sending emails.
    /// </summary>
    protected SmtpAccount Account { get; } = account;

    /// <summary>
    /// The default "From" address.
    /// </summary>
    public MailAddress? DefaultFrom { get; set; }

    /// <summary>
    /// The default set of recipients.
    /// </summary>
    public IEnumerable<MailAddress> DefaultRecipients { get; set; } = [];

    /// <summary>
    /// The default set of CC recipients.
    /// </summary>
    public IEnumerable<MailAddress> DefaultCc { get; } = [];

    /// <summary>
    /// The default set of BCC recipients.
    /// </summary>
    public IEnumerable<MailAddress> DefaultBcc { get; } = [];

    /// <summary>
    /// The default set of reply-to addresses.
    /// </summary>
    public IEnumerable<MailAddress> DefaultReplyTo { get; } = [];

    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        AppendDefaults(message.Headers);

        ThrowIfInvalid(message);

        return SendInternalAsync(message, cancellationToken);
    }

    /// <summary>
    /// Adds the default values to the mail headers.
    /// </summary>
    /// <param name="mailHeaders">The mail headers to update.</param>
    protected virtual void AppendDefaults(MailHeaderCollection mailHeaders)
    {
        if (DefaultFrom is not null)
            mailHeaders.From ??= DefaultFrom;

        if (DefaultRecipients.Any())
            mailHeaders.AddRecipient(DefaultRecipients);

        if (DefaultCc.Any())
            mailHeaders.AddCarbonCopy(DefaultCc);

        if (DefaultBcc.Any())
            mailHeaders.AddBlindCarbonCopy(DefaultBcc);

        if (DefaultReplyTo.Any())
            mailHeaders.AddReplyTo(DefaultReplyTo);
    }

    protected virtual void ThrowIfInvalid(MailMessage message)
    {
        if (message.Headers.From is null)
            throw new InvalidOperationException("The 'From' address must be set.");

        if (!message.Headers.Recipients.Any())
            throw new InvalidOperationException("At least one recipient must be specified.");

        if (string.IsNullOrWhiteSpace(message.Content.HtmlBody))
            throw new InvalidOperationException("The HTML body must be set.");

        if (string.IsNullOrWhiteSpace(message.Content.Subject))
            throw new InvalidOperationException("The subject must be set.");
    }

    /// <summary>
    /// Sends the mail message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected abstract Task SendInternalAsync(MailMessage message, CancellationToken cancellationToken);
}
