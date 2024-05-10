namespace TRENZ.Lib.RazorMail.Models;

/// <summary>
/// Represents an SMTP account.
/// </summary>
public record SmtpAccount
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "SmtpAccount";

    /// <summary>
    /// The host of the SMTP server.
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    /// The port of the SMTP server.
    /// </summary>
    public required int Port { get; init; }

    /// <summary>
    /// Indicates whether the SMTP server requires TLS.
    /// </summary>
    public required bool TLS { get; init; } = true;

    /// <summary>
    /// The login to authenticate with the SMTP server.
    /// </summary>
    public required string Login { get; init; }

    /// <summary>
    /// The password to authenticate with the SMTP server.
    /// </summary>
    public required string Password { get; init; }
}
