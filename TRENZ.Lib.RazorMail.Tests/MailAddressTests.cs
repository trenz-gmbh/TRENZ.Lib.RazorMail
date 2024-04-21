using System;

using NUnit.Framework;

namespace TRENZ.Lib.RazorMail.Models;

public class MailAddressTests
{
    private const string _EXPECTED_ADDRESS = "a@example.com";
    private const string _INVALID_ADDRESS = "aexample.com";
    private const string _EXPECTED_NAME = "John Doe";

    [Test]
    public void CtorWithAddress()
    {
        var address = new MailAddress(_EXPECTED_ADDRESS);
        Assert.That(_EXPECTED_ADDRESS, Is.EqualTo(address.Email));
    }

    [Test]
    public void CtorWithBoth()
    {
        var address = new MailAddress(_EXPECTED_ADDRESS, _EXPECTED_NAME);
        Assert.That(_EXPECTED_ADDRESS, Is.EqualTo(address.Email));
        Assert.That(_EXPECTED_NAME, Is.EqualTo(address.Name));
    }

    [Test]
    public void CastFromString()
    {
        MailAddress address = _EXPECTED_ADDRESS;
        Assert.That(_EXPECTED_ADDRESS, Is.EqualTo(address.Email));
    }

    [Test]
    public void CastToString()
    {
        var address = new MailAddress(_EXPECTED_ADDRESS);
        var addressAsString = (string)address;
        Assert.That(_EXPECTED_ADDRESS, Is.EqualTo(addressAsString));
    }

    [Test]
    public void InvalidAddress()
    {
        Assert.Throws<FormatException>(() => new MailAddress(_INVALID_ADDRESS));
    }
}