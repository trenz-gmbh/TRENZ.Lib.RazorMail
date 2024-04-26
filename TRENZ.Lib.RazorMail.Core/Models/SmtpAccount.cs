namespace TRENZ.Lib.RazorMail.Models;

/// <summary>
/// Represents an SMTP account.
/// </summary>
/// <param name="Host">The SMTP server host.</param>
/// <param name="Port">The SMTP server port.</param>
/// <param name="TLS">Whether to use TLS.</param>
/// <param name="Login">The login name.</param>
/// <param name="Password">The password.</param>
public record SmtpAccount(
    string Host,
    int Port,
    bool TLS,
    string Login,
    string Password
);
