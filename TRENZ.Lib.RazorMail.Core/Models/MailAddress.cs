using System;
using System.Diagnostics;

namespace TRENZ.Lib.RazorMail.Models;

/// <summary>
/// Represents an e-mail address with an optional display name.
/// </summary>
[DebuggerDisplay("Email = {Email}, Name = {Name}")]
public class MailAddress
{
    private static string _ValidateEmail(string email)
    {
        if (!email.Contains('@'))
            throw new FormatException($"E-mail address '{email}' doesn't look valid");

        return email;
    }

    /// <summary>
    /// The actual e-mail address.
    /// </summary>
    public required string Email
    {
        get => email ?? throw new InvalidOperationException("E-mail address is not set");
        init => email = _ValidateEmail(value);
    }

    private readonly string? email;

    /// <summary>
    /// The display name of the e-mail address.
    /// </summary>
    public string? Name { get; init; }

    public static implicit operator string(MailAddress address) => address.Email;

    public static implicit operator MailAddress(string address) => new() { Email = address };

    public override string ToString() => string.IsNullOrWhiteSpace(Name) ? Email : $"{Name} <{Email}>";
}
