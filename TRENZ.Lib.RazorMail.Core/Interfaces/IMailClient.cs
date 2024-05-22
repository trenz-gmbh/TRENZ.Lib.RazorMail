using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Interfaces;

/// <summary>
/// Represents a client that sends email messages and stores default From, To, CC, BCC, and Reply-To addresses.
/// </summary>
public interface IMailClient
{
    /// <summary>
    /// The default headers to add to each mail message.
    /// </summary>
    public MailHeaderCollection DefaultHeaders { get; }

    /// <summary>
    /// The default "From" address. Can be overriden by the message.
    /// </summary>
    /// <remarks>
    /// If this is <see langword="null"/>, it must be set in the <see cref="MailMessage"/>, otherwise
    /// <see cref="SendAsync"/> will throw a <see cref="System.InvalidOperationException"/>.
    /// </remarks>
    public MailAddress? DefaultFrom {
        get => DefaultHeaders.From;
        set => DefaultHeaders.From = value;
    }

    /// <summary>
    /// The default set of recipients. Will be added to the message recipients.
    /// </summary>
    public IEnumerable<MailAddress> DefaultRecipients {
        get => DefaultHeaders.Recipients;
        set => DefaultHeaders.Recipients = value;
    }

    /// <summary>
    /// The default set of CC recipients. Will be added to the message CC recipients.
    /// </summary>
    public IEnumerable<MailAddress> DefaultCc {
        get => DefaultHeaders.CarbonCopy;
        set => DefaultHeaders.CarbonCopy = value;
    }

    /// <summary>
    /// The default set of BCC recipients. Will be added to the message BCC recipients.
    /// </summary>
    public IEnumerable<MailAddress> DefaultBcc {
        get => DefaultHeaders.BlindCarbonCopy;
        set => DefaultHeaders.BlindCarbonCopy = value;
    }

    /// <summary>
    /// The default set of reply-to addresses. Will be added to the message reply-to addresses.
    /// </summary>
    public IEnumerable<MailAddress> DefaultReplyTo {
        get => DefaultHeaders.ReplyTo;
        set => DefaultHeaders.ReplyTo = value;
    }

    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Adds the Default* properties from this sender to the message and validates if the mail contains a From address,
    /// at least one recipient and a body.
    /// </remarks>
    Task SendAsync(MailMessage message, CancellationToken cancellationToken = default);
}
