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
    /// Creates a new instance of the <see cref="MailAddress"/> class with the specified e-mail address.
    /// </summary>
    /// <param name="address">The e-mail address.</param>
    public MailAddress(string address) => Email = _ValidateEmail(address);

    /// <summary>
    /// Creates a new instance of the <see cref="MailAddress"/> class with the specified e-mail address and display name.
    /// </summary>
    /// <param name="address">The e-mail address.</param>
    /// <param name="displayName">The display name of the e-mail address.</param>
    public MailAddress(string address, string displayName) : this(address) => Name = displayName;

    /// <summary>
    /// The actual e-mail address.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// The display name of the e-mail address.
    /// </summary>
    public string? Name { get; init; }

    public static implicit operator string(MailAddress address) => address.Email;

    public static implicit operator MailAddress(string address) => new(address);

    public override string ToString() => string.IsNullOrWhiteSpace(Name) ? Email : $"{Name} <{Email}>";
}
