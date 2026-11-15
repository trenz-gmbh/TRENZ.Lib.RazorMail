using MailKit.Net.Smtp;

using Microsoft.Extensions.Logging.Abstractions;

using TRENZ.Lib.RazorMail.MailKit;

namespace TRENZ.Lib.RazorMail.Tests;

/// <summary>
/// A <see cref="MailKitMailClient"/> that hands out the supplied <see cref="FakeSmtpClient"/>
/// instead of creating a real one.
/// </summary>
internal sealed class TestableMailKitMailClient(FakeSmtpClient client)
    : MailKitMailClient(TestSmtpAccounts.CreateOptions(), NullLogger<MailKitMailClient>.Instance)
{
    protected override SmtpClient CreateSmtpClient() => client;
}
