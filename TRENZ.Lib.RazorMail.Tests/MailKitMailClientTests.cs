using System;
using System.Threading.Tasks;

using MailKit.Net.Smtp;
using MailKit.Security;

using NUnit.Framework;

namespace TRENZ.Lib.RazorMail.Tests;

public class MailKitMailClientTests
{
    [Test]
    public void DisposesClientWhenAuthenticationFails()
    {
        var expected = new AuthenticationException("invalid credentials");
        var fake = new FakeSmtpClient { AuthenticateException = expected };
        var sut = new TestableMailKitMailClient(fake);

        var actual = Assert.ThrowsAsync<AuthenticationException>(() => sut.SendAsync(TestMessages.CreateValid()));

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

        var actual = Assert.ThrowsAsync<SmtpProtocolException>(() => sut.SendAsync(TestMessages.CreateValid()));

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

        Assert.ThrowsAsync<OperationCanceledException>(() => sut.SendAsync(TestMessages.CreateValid()));

        Assert.That(fake.DisposeCount, Is.EqualTo(1));
    }

    [Test]
    public async Task DisposesClientExactlyOnceOnSuccess()
    {
        var fake = new FakeSmtpClient();
        var sut = new TestableMailKitMailClient(fake);

        await sut.SendAsync(TestMessages.CreateValid());

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
