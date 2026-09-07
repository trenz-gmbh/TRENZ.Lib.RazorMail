using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MSGraph.Extensions;

public static class MailMessageExtensions
{
    public static SendMailPostRequestBody ToMSMailPostRequestBody(this MailMessage message,
        bool saveToSentItems = false)
    {
        var messageContent = message.Content;
        var messageHeader = message.Headers;
        var msGraphRecipients = ExtractRecipientsFromHeader(messageHeader);

        return new SendMailPostRequestBody
        {
            Message = new Message
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
                Importance = messageHeader.Importance.ToMSImportance(),
                From = new Recipient()
                {
                    EmailAddress = messageHeader.From?.ToMSEmailAddress(),
                }
            },
            SaveToSentItems = saveToSentItems
        };
    }

    private static MSRecipients ExtractRecipientsFromHeader(MailHeaderCollection headerCollection)
    {
        var msRecipients = new MSRecipients();
        foreach (var domainRecipients in headerCollection.Recipients)
        {
            msRecipients.ToRecipients.Add(
                new Recipient
                {
                    EmailAddress = domainRecipients.ToMSEmailAddress()
                }
            );
        }

        foreach (var domainRecipients in headerCollection.CarbonCopy)
        {
            msRecipients.CcRecipients.Add(
                new Recipient
                {
                    EmailAddress = domainRecipients.ToMSEmailAddress()
                }
            );
        }

        foreach (var domainRecipients in headerCollection.BlindCarbonCopy)
        {
            msRecipients.BccRecipients.Add(
                new Recipient
                {
                    EmailAddress = domainRecipients.ToMSEmailAddress()
                }
            );
        }

        foreach (var domainRecipients in headerCollection.ReplyTo)
        {
            msRecipients.ReplyRecipients.Add(
                new Recipient
                {
                    EmailAddress = domainRecipients.ToMSEmailAddress()
                }
            );
        }

        return msRecipients;
    }


    private class MSRecipients
    {
        public List<Recipient> ToRecipients { get; } = new();
        public List<Recipient> CcRecipients { get; } = new();
        public List<Recipient> BccRecipients { get; } = new();
        public List<Recipient> ReplyRecipients { get; } = new();
    }
}
