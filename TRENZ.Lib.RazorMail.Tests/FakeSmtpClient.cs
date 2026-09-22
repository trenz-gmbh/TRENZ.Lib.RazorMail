using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit;

namespace TRENZ.Lib.RazorMail.Tests;

/// <summary>
/// A <see cref="SmtpClient"/> that never touches a socket. It records how often the
/// connect/authenticate/disconnect/send members were used and how often it was disposed,
/// and can be told to fail at a specific point of the connection setup.
/// </summary>
internal sealed class FakeSmtpClient : SmtpClient
{
    public Exception? ConnectException { get; init; }

    public Exception? AuthenticateException { get; init; }

    public int ConnectCount { get; private set; }

    public int AuthenticateCount { get; private set; }

    public int SendCount { get; private set; }

    public int DisconnectCount { get; private set; }

    public int DisposeCount { get; private set; }

    public override Task ConnectAsync(string host, int port = 0,
        SecureSocketOptions options = SecureSocketOptions.Auto,
        CancellationToken cancellationToken = default)
    {
        ConnectCount++;

        if (ConnectException is not null)
            return Task.FromException(ConnectException);

        return Task.CompletedTask;
    }

    public override Task AuthenticateAsync(Encoding encoding, ICredentials credentials,
        CancellationToken cancellationToken = default)
    {
        AuthenticateCount++;

        if (AuthenticateException is not null)
            return Task.FromException(AuthenticateException);

        return Task.CompletedTask;
    }

    public override Task<string> SendAsync(FormatOptions options, MimeMessage message,
        CancellationToken cancellationToken = default, ITransferProgress? progress = null)
    {
        SendCount++;

        return Task.FromResult("250 OK");
    }

    public override Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default)
    {
        DisconnectCount++;

        return Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        DisposeCount++;

        base.Dispose(disposing);
    }
}
