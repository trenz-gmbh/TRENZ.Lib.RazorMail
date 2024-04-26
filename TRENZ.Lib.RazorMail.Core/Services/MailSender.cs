using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Services;

/// <summary>
/// Creates a new mail sender with the specified SMTP account.
/// </summary>
/// <param name="account">The account to use for sending emails.</param>
public abstract class MailSender(SmtpAccount account)
{
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
    public abstract Task SendAsync(MailMessage message, CancellationToken cancellationToken = default);
}
