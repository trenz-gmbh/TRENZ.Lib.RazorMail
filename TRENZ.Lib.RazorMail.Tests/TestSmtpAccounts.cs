using Microsoft.Extensions.Options;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Tests;

/// <summary>
/// Builds <see cref="SmtpAccount"/> values for mail clients that never open a socket, so the
/// individual settings are arbitrary and only have to be present.
/// </summary>
internal static class TestSmtpAccounts
{
    /// <summary>
    /// Creates a dummy account pointing at a server that is never contacted.
    /// </summary>
    public static SmtpAccount Create() => new()
    {
        Host = "localhost",
        Port = 25,
        TLS = false,
        Login = "login",
        Password = "password",
    };

    /// <summary>
    /// Creates a dummy account wrapped in the <see cref="IOptions{TOptions}"/> the mail clients take.
    /// </summary>
    public static IOptions<SmtpAccount> CreateOptions() => Options.Create(Create());
}
