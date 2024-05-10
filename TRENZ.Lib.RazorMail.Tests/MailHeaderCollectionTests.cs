using NUnit.Framework;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Tests;

public class MailHeaderCollectionTests
{
    [Test]
    public void TestAppendDoesntOverwriteExistingValues()
    {
        var collection = new MailHeaderCollection
        {
            { "key", "value" },
        };

        collection.Append(new()
        {
            { "key", "new value" },
        });

        Assert.That(collection["key"], Is.EqualTo("value"));
    }

    [Test]
    public void TestAppendDoesntRemoveAddresses()
    {
        var me = new MailAddress("me@example.test");
        var you = new MailAddress("you@example.test");
        var anna = new MailAddress("anna@example.test");
        var brad = new MailAddress("brad@example.test");
        var charlie = new MailAddress("charlie@example.test");

        var me2 = new MailAddress("me2@example.test");
        var you2 = new MailAddress("you2@example.test");
        var anna2 = new MailAddress("anna2@example.test");
        var brad2 = new MailAddress("brad2@example.test");
        var charlie2 = new MailAddress("charlie2@example.test");

        var collection = new MailHeaderCollection
        {
            From = me,
            Recipients = [you],
            CarbonCopy = [anna],
            BlindCarbonCopy = [brad],
            ReplyTo = [charlie],
        };

        collection.Append(new()
        {
            From = me2,
            Recipients = [you2],
            CarbonCopy = [anna2],
            BlindCarbonCopy = [brad2],
            ReplyTo = [charlie2],
        });

        Assert.Multiple(() =>
        {
            Assert.That(collection.From, Is.EqualTo(me));
            Assert.That(collection.Recipients, Is.EquivalentTo(new [] { you, you2 }));
            Assert.That(collection.CarbonCopy, Is.EquivalentTo(new [] { anna, anna2 }));
            Assert.That(collection.BlindCarbonCopy, Is.EquivalentTo(new [] { brad, brad2 }));
            Assert.That(collection.ReplyTo, Is.EquivalentTo(new [] { charlie, charlie2 }));
        });
    }

    [Test]
    [TestCase("Key", "Key")]
    [TestCase("Key", "key")]
    [TestCase("Key", "KEY")]
    public void TestHeaderKeysAreCaseInsensitive(string setKey, string getKey)
    {
        var collection = new MailHeaderCollection
        {
            { setKey, "value" },
        };

        Assert.That(collection[getKey], Is.EqualTo("value"));
    }

    [Test]
    public void TestHeaderKeyCasesArePreserved()
    {
        var collection = new MailHeaderCollection
        {
            { "Key", "value" },
            { "key2", "value" },
            { "KEY3", "value" },
        };

        Assert.That(collection.Keys, Is.EquivalentTo(new [] { "Key", "key2", "KEY3"}));
    }
}
