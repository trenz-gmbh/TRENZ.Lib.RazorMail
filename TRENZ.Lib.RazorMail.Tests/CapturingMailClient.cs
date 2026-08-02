using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.Services;

namespace TRENZ.Lib.RazorMail.Tests;

/// <summary>
/// A <see cref="BaseSmtpMailClient"/> that captures the messages it is asked to send
/// instead of talking to an SMTP server.
/// </summary>
internal sealed class CapturingMailClient(IOptions<SmtpAccount> accountOptions)
    : BaseSmtpMailClient(accountOptions)
{
    public List<MailMessage> SentMessages { get; } = [];

    protected override Task SendInternalAsync(MailMessage message, CancellationToken cancellationToken)
    {
        SentMessages.Add(message);

        return Task.CompletedTask;
    }
}
