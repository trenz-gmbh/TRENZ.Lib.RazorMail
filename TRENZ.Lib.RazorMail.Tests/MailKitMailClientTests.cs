using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using MimeKit;

using NUnit.Framework;

using TRENZ.Lib.RazorMail.MailKit;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Tests;

public class MailKitMailClientTests
{
    /// <summary>
    /// A <see cref="SmtpClient"/> that never touches a socket. It records how often the
    /// connect/authenticate/disconnect/send members were used and how often it was disposed,
    /// and can be told to fail at a specific point of the connection setup.
    /// </summary>
    private sealed class FakeSmtpClient : SmtpClient
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

    /// <summary>
    /// A <see cref="MailKitMailClient"/> that hands out the supplied <see cref="FakeSmtpClient"/>
    /// instead of creating a real one.
    /// </summary>
    private sealed class TestableMailKitMailClient(FakeSmtpClient client)
        : MailKitMailClient(
            Options.Create(new SmtpAccount
            {
                Host = "smtp.example.com",
                Port = 587,
                TLS = true,
                Login = "user",
                Password = "secret",
            }),
            NullLogger<MailKitMailClient>.Instance)
    {
        protected override SmtpClient CreateSmtpClient() => client;
    }

    private static MailMessage CreateValidMessage()
    {
        var headers = new MailHeaderCollection
        {
            From = new("sender@example.com", "Sender"),
        };

        headers.AddRecipient(new MailAddress("recipient@example.com", "Recipient"));

        return new()
        {
            Headers = headers,
            Content = new()
            {
                Subject = "Subject",
                HtmlBody = "<p>Hello</p>",
            },
        };
    }

    [Test]
    public void DisposesClientWhenAuthenticationFails()
    {
        var expected = new AuthenticationException("invalid credentials");
        var fake = new FakeSmtpClient { AuthenticateException = expected };
        var sut = new TestableMailKitMailClient(fake);

        var actual = Assert.ThrowsAsync<AuthenticationException>(() => sut.SendAsync(CreateValidMessage()));

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.SameAs(expected));
            Assert.That(fake.ConnectCount, Is.EqualTo(1));
            Assert.That(fake.AuthenticateCount, Is.EqualTo(1));
            Assert.That(fake.DisposeCount, Is.EqualTo(1));
            Assert.That(fake.SendCount, Is.Zero);
        });
    }

    [Test]
    public void DisposesClientWhenConnectFails()
    {
        var expected = new SmtpProtocolException("unexpected greeting");
        var fake = new FakeSmtpClient { ConnectException = expected };
        var sut = new TestableMailKitMailClient(fake);

        var actual = Assert.ThrowsAsync<SmtpProtocolException>(() => sut.SendAsync(CreateValidMessage()));

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.SameAs(expected));
            Assert.That(fake.ConnectCount, Is.EqualTo(1));
            Assert.That(fake.AuthenticateCount, Is.Zero);
            Assert.That(fake.DisposeCount, Is.EqualTo(1));
            Assert.That(fake.SendCount, Is.Zero);
        });
    }

    [Test]
    public void DisposesClientWhenAuthenticationIsCancelled()
    {
        var fake = new FakeSmtpClient { AuthenticateException = new OperationCanceledException() };
        var sut = new TestableMailKitMailClient(fake);

        Assert.ThrowsAsync<OperationCanceledException>(() => sut.SendAsync(CreateValidMessage()));

        Assert.That(fake.DisposeCount, Is.EqualTo(1));
    }

    [Test]
    public async Task DisposesClientExactlyOnceOnSuccess()
    {
        var fake = new FakeSmtpClient();
        var sut = new TestableMailKitMailClient(fake);

        await sut.SendAsync(CreateValidMessage());

        Assert.Multiple(() =>
        {
            Assert.That(fake.ConnectCount, Is.EqualTo(1));
            Assert.That(fake.AuthenticateCount, Is.EqualTo(1));
            Assert.That(fake.SendCount, Is.EqualTo(1));
            Assert.That(fake.DisconnectCount, Is.EqualTo(1));
            Assert.That(fake.DisposeCount, Is.EqualTo(1));
        });
    }
}
