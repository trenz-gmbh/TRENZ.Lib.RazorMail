using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MicrosoftGraph.Extensions;

public static class MailMessageExtensions
{
    public static Message ToMsMessage(this MailMessage message)
    {
        var messageContent = message.Content;
        var messageHeader = message.Headers;
        var msGraphRecipients = ExtractRecipientsFromHeader(messageHeader);
        return new Message
        {
            Subject = messageContent.Subject,
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = messageContent.HtmlBody
            },
            ToRecipients = msGraphRecipients.ToRecipients,
            CcRecipients = msGraphRecipients.CcRecipients,
            BccRecipients = msGraphRecipients.BccRecipients,
            ReplyTo = msGraphRecipients.ReplyRecipients,
            Importance = messageHeader.Importance.ToMsImportance(),
            From = new Recipient
            {
                EmailAddress = messageHeader.From?.ToMsEmailAddress()
            },
            AdditionalData = messageHeader.NonSpecificHandledHeaders as IDictionary<string, object> ??
                             new Dictionary<string, object>()
        };
    }

    public static SendMailPostRequestBody ToMsMailPostRequestBody(this Message message,
        bool saveToSentItems = false)
    {
        return new SendMailPostRequestBody
        {
            Message = message,
            SaveToSentItems = saveToSentItems
        };
    }

    private static MsRecipients ExtractRecipientsFromHeader(MailHeaderCollection headerCollection)
    {
        var msRecipients = new MsRecipients();
        msRecipients.ToRecipients.AddRange(headerCollection.Recipients.Select(mailAddress => new Recipient
        {
            EmailAddress = mailAddress.ToMsEmailAddress()
        }));
        msRecipients.CcRecipients.AddRange(headerCollection.CarbonCopy.Select(mailAddress => new Recipient
        {
            EmailAddress = mailAddress.ToMsEmailAddress()
        }));
        msRecipients.BccRecipients.AddRange(headerCollection.BlindCarbonCopy.Select(mailAddress => new Recipient
        {
            EmailAddress = mailAddress.ToMsEmailAddress()
        }));
        msRecipients.ReplyRecipients.AddRange(headerCollection.ReplyTo.Select(mailAddress => new Recipient
        {
            EmailAddress = mailAddress.ToMsEmailAddress()
        }));
        return msRecipients;
    }

    private class MsRecipients
    {
        public List<Recipient> ToRecipients { get; } = [];
        public List<Recipient> CcRecipients { get; } = [];
        public List<Recipient> BccRecipients { get; } = [];
        public List<Recipient> ReplyRecipients { get; } = [];
    }
}
