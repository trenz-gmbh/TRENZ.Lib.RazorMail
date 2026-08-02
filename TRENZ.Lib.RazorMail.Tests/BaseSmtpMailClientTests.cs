using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Tests;

public class BaseSmtpMailClientTests
{
    private static CapturingMailClient CreateClient() => new(TestSmtpAccounts.CreateOptions());

    private static string[] Emails(IEnumerable<MailAddress> addresses) => addresses.Select(a => a.Email).ToArray();

    [Test]
    public async Task TestSendingTheSameMessageTwiceDoesntDuplicateDefaults()
    {
        var client = CreateClient();
        IMailClient defaults = client;
        defaults.DefaultFrom = new MailAddress("sender@example.test");
        defaults.DefaultRecipients = [new MailAddress("default-to@example.test")];
        defaults.DefaultCc = [new MailAddress("default-cc@example.test")];

        var message = TestMessages.Create(new MailHeaderCollection
        {
            Recipients = [new MailAddress("you@example.test")],
        });

        await client.SendAsync(message);
        await client.SendAsync(message);

        Assert.That(client.SentMessages, Has.Count.EqualTo(2));

        var first = client.SentMessages[0];
        var second = client.SentMessages[1];

        Assert.Multiple(() =>
        {
            Assert.That(Emails(second.Headers.Recipients), Is.EquivalentTo(Emails(first.Headers.Recipients)));
            Assert.That(Emails(second.Headers.CarbonCopy), Is.EquivalentTo(Emails(first.Headers.CarbonCopy)));
            Assert.That(Emails(second.Headers.Recipients),
                Is.EquivalentTo(new[] { "you@example.test", "default-to@example.test" }));
            Assert.That(Emails(second.Headers.CarbonCopy), Is.EquivalentTo(new[] { "default-cc@example.test" }));
        });
    }

    [Test]
    public async Task TestSendDoesntMutateCallersMessage()
    {
        var client = CreateClient();
        IMailClient defaults = client;
        defaults.DefaultFrom = new MailAddress("sender@example.test");
        defaults.DefaultRecipients = [new MailAddress("default-to@example.test")];
        defaults.DefaultCc = [new MailAddress("default-cc@example.test")];
        client.DefaultHeaders["X-Default"] = "yes";

        var headers = new MailHeaderCollection
        {
            Recipients = [new MailAddress("you@example.test")],
        };

        var message = TestMessages.Create(headers);

        await client.SendAsync(message);

        Assert.Multiple(() =>
        {
            Assert.That(message.Headers, Is.SameAs(headers));
            Assert.That(Emails(message.Headers.Recipients), Is.EquivalentTo(new[] { "you@example.test" }));
            Assert.That(message.Headers.CarbonCopy, Is.Empty);
            Assert.That(message.Headers.From, Is.Null);
            Assert.That(message.Headers.ContainsKey("X-Default"), Is.False);
            Assert.That(client.SentMessages[0].Headers, Is.Not.SameAs(headers));
        });
    }

    [Test]
    public async Task TestDefaultHeadersAreStillMerged()
    {
        var client = CreateClient();
        IMailClient defaults = client;
        defaults.DefaultFrom = new MailAddress("sender@example.test");
        defaults.DefaultRecipients = [new MailAddress("default-to@example.test")];
        defaults.DefaultCc = [new MailAddress("default-cc@example.test")];
        defaults.DefaultBcc = [new MailAddress("default-bcc@example.test")];
        defaults.DefaultReplyTo = [new MailAddress("default-reply-to@example.test")];
        client.DefaultHeaders["X-Default"] = "yes";

        var message = TestMessages.Create(new MailHeaderCollection
        {
            Recipients = [new MailAddress("you@example.test")],
        });

        await client.SendAsync(message);

        var sent = client.SentMessages[0];

        Assert.Multiple(() =>
        {
            Assert.That(sent.Content, Is.SameAs(message.Content));
            Assert.That(sent.Headers.From?.Email, Is.EqualTo("sender@example.test"));
            Assert.That(Emails(sent.Headers.Recipients),
                Is.EquivalentTo(new[] { "you@example.test", "default-to@example.test" }));
            Assert.That(Emails(sent.Headers.CarbonCopy), Is.EquivalentTo(new[] { "default-cc@example.test" }));
            Assert.That(Emails(sent.Headers.BlindCarbonCopy), Is.EquivalentTo(new[] { "default-bcc@example.test" }));
            Assert.That(Emails(sent.Headers.ReplyTo), Is.EquivalentTo(new[] { "default-reply-to@example.test" }));
            Assert.That(sent.Headers["X-Default"], Is.EqualTo("yes"));
        });
    }

    [Test]
    public async Task TestMessageHeadersWinOverDefaults()
    {
        var client = CreateClient();
        IMailClient defaults = client;
        defaults.DefaultFrom = new MailAddress("default-sender@example.test");

        var message = TestMessages.Create(new MailHeaderCollection
        {
            From = new MailAddress("explicit-sender@example.test"),
            Recipients = [new MailAddress("you@example.test")],
        });

        await client.SendAsync(message);

        Assert.That(client.SentMessages[0].Headers.From?.Email, Is.EqualTo("explicit-sender@example.test"));
    }

    [Test]
    public async Task TestChangingDefaultsAfterSendDoesntChangeSentMessage()
    {
        var client = CreateClient();
        IMailClient defaults = client;
        defaults.DefaultFrom = new MailAddress("sender@example.test");

        var defaultRecipients = new List<MailAddress> { new("default-to@example.test") };
        defaults.DefaultRecipients = defaultRecipients;

        var message = TestMessages.Create(new MailHeaderCollection
        {
            Recipients = [new MailAddress("you@example.test")],
        });

        await client.SendAsync(message);

        defaultRecipients.Add(new MailAddress("late-addition@example.test"));

        Assert.That(Emails(client.SentMessages[0].Headers.Recipients),
            Is.EquivalentTo(new[] { "you@example.test", "default-to@example.test" }));
    }

    [Test]
    public void TestAddRecipientDoesntAliasTheSourceCollection()
    {
        var source = new List<MailAddress> { new("you@example.test") };

        var collection = new MailHeaderCollection();
        collection.AddRecipient(source);

        source.Add(new MailAddress("sneaky@example.test"));

        Assert.That(Emails(collection.Recipients), Is.EquivalentTo(new[] { "you@example.test" }));
    }

    [Test]
    public void TestSettingAddressesDoesntAliasTheSourceCollection()
    {
        var recipients = new List<MailAddress> { new("you@example.test") };
        var carbonCopy = new List<MailAddress> { new("anna@example.test") };

        var collection = new MailHeaderCollection
        {
            Recipients = recipients,
            CarbonCopy = carbonCopy,
        };

        recipients.Add(new MailAddress("sneaky@example.test"));
        carbonCopy.Add(new MailAddress("sneaky-cc@example.test"));

        Assert.Multiple(() =>
        {
            Assert.That(Emails(collection.Recipients), Is.EquivalentTo(new[] { "you@example.test" }));
            Assert.That(Emails(collection.CarbonCopy), Is.EquivalentTo(new[] { "anna@example.test" }));
        });
    }
}
