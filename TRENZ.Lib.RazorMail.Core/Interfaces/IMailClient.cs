using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Interfaces;

public interface IMailClient
{
    /// <summary>
    /// The default "From" address.
    /// </summary>
    MailAddress? DefaultFrom { get; set; }

    /// <summary>
    /// The default set of recipients.
    /// </summary>
    IEnumerable<MailAddress> DefaultRecipients { get; set; }

    /// <summary>
    /// The default set of CC recipients.
    /// </summary>
    IEnumerable<MailAddress> DefaultCc { get; set; }

    /// <summary>
    /// The default set of BCC recipients.
    /// </summary>
    IEnumerable<MailAddress> DefaultBcc { get; set; }

    /// <summary>
    /// The default set of reply-to addresses.
    /// </summary>
    IEnumerable<MailAddress> DefaultReplyTo { get; set; }

    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendAsync(MailMessage message, CancellationToken cancellationToken = default);
}
