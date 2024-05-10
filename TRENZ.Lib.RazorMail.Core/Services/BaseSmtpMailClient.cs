using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Services;

/// <summary>
/// Creates a new mail sender with the specified SMTP accountOptions.
/// </summary>
/// <param name="accountOptions">The accountOptions to use for sending emails.</param>
public abstract class BaseSmtpMailClient(IOptions<SmtpAccount> accountOptions) : IMailClient
{
    /// <summary>
    /// The SMTP account to use for sending emails.
    /// </summary>
    protected SmtpAccount Account => accountOptions.Value;

    /// <inheritdoc />
    public MailHeaderCollection DefaultHeaders { get; } = new();

    /// <inheritdoc />
    public Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        message.Headers.AppendFrom(DefaultHeaders);

        ThrowIfInvalid(message);

        return SendInternalAsync(message, cancellationToken);
    }

    protected virtual void ThrowIfInvalid(MailMessage message)
    {
        if (message.Headers.From is null)
            throw new InvalidOperationException("The 'From' address must be set.");

        if (!message.Headers.Recipients.Any())
            throw new InvalidOperationException("At least one recipient must be specified.");

        if (string.IsNullOrWhiteSpace(message.Content.HtmlBody))
            throw new InvalidOperationException("The HTML body must be set.");
    }

    /// <summary>
    /// Sends the mail message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected abstract Task SendInternalAsync(MailMessage message, CancellationToken cancellationToken);
}
