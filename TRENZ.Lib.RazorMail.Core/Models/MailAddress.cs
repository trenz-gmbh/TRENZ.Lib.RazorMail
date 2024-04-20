using System;
using System.Diagnostics;

namespace TRENZ.Lib.RazorMail.Models;

[DebuggerDisplay("Email = {Email}, Name = {Name}")]
public class MailAddress
{
    public MailAddress(string email)
    {
        _ValidateEmail(email);

        Email = email;
    }

    public MailAddress(string email, string name)
    {
        _ValidateEmail(email);

        Email = email;
        Name = name;
    }

    private static void _ValidateEmail(string email)
    {
        if (!email.Contains("@"))
            throw new FormatException($"E-mail address '{email}' doesn't look valid");
    }

    public string Email { get; set; }
    public string Name { get; set; } = "";

    public static implicit operator string(MailAddress address)
        => address.Email;

    public static implicit operator MailAddress(string address)
        => new MailAddress(address, "");

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return Email;

        return $"{Name} <{Email}>";
    }
}