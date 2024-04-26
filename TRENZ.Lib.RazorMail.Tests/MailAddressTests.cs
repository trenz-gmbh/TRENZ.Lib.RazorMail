using System;

using NUnit.Framework;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Tests;

public class MailAddressTests
{
    private const string _EXPECTED_ADDRESS = "a@example.com";
    private const string _INVALID_ADDRESS = "aexample.com";
    private const string _EXPECTED_NAME = "John Doe";

    [Test]
    public void CtorWithAddress()
    {
        var address = new MailAddress(_EXPECTED_ADDRESS);
        
        Assert.That(address.Email, Is.EqualTo(_EXPECTED_ADDRESS));
    }

    [Test]
    public void CtorWithBoth()
    {
        var address = new MailAddress(_EXPECTED_ADDRESS, _EXPECTED_NAME);
        
        Assert.Multiple(() =>
        {
            Assert.That(address.Email, Is.EqualTo(_EXPECTED_ADDRESS));
            Assert.That(address.Name, Is.EqualTo(_EXPECTED_NAME));
        });
    }

    [Test]
    public void CastFromString()
    {
        MailAddress address = _EXPECTED_ADDRESS;
        
        Assert.That(address.Email, Is.EqualTo(_EXPECTED_ADDRESS));
    }

    [Test]
    public void CastToString()
    {
        var address = new MailAddress(_EXPECTED_ADDRESS);
        var addressAsString = (string)address;
      
        Assert.That(addressAsString, Is.EqualTo(_EXPECTED_ADDRESS));
    }

    [Test]
    public void InvalidAddress()
    {
        Assert.Throws<FormatException>(() => _ = new MailAddress(_INVALID_ADDRESS));
    }
}