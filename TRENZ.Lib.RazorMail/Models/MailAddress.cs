using System;
using System.Diagnostics;

using MimeKit;

using NUnit.Framework;

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

    public static implicit operator System.Net.Mail.MailAddress(MailAddress address)
        => new System.Net.Mail.MailAddress(address.Email, address.Name);

    public MailboxAddress ToMailboxAddress()
        => new MailboxAddress(Name, Email);

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return Email;

        return $"{Name} <{Email}>";
    }
}

public class MailAddressTests
{
    private const string _EXPECTED_ADDRESS = "a@example.com";
    private const string _INVALID_ADDRESS = "aexample.com";
    private const string _EXPECTED_NAME = "John Doe";

    [Test]
    public void CtorWithAddress()
    {
        var address = new MailAddress(_EXPECTED_ADDRESS);
        Assert.AreEqual(_EXPECTED_ADDRESS, address.Email);
    }

    [Test]
    public void CtorWithBoth()
    {
        var address = new MailAddress(_EXPECTED_ADDRESS, _EXPECTED_NAME);
        Assert.AreEqual(_EXPECTED_ADDRESS, address.Email);
        Assert.AreEqual(_EXPECTED_NAME, address.Name);
    }

    [Test]
    public void CastFromString()
    {
        MailAddress address = _EXPECTED_ADDRESS;
        Assert.AreEqual(_EXPECTED_ADDRESS, address.Email);
    }

    [Test]
    public void CastToString()
    {
        var address = new MailAddress(_EXPECTED_ADDRESS);
        var addressAsString = (string)address;
        Assert.AreEqual(_EXPECTED_ADDRESS, addressAsString);
    }

    [Test]
    public void InvalidAddress()
    {
        Assert.Throws<FormatException>(() => new MailAddress(_INVALID_ADDRESS));
    }
}