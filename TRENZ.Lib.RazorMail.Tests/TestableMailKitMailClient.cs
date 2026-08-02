using MailKit.Net.Smtp;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using TRENZ.Lib.RazorMail.MailKit;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Tests;

/// <summary>
/// A <see cref="MailKitMailClient"/> that hands out the supplied <see cref="FakeSmtpClient"/>
/// instead of creating a real one.
/// </summary>
internal sealed class TestableMailKitMailClient(FakeSmtpClient client)
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
