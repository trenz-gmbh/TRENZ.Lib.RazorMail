using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MicrosoftGraph.Extensions;

public static class MailMessageExtensions
{
    public static SendMailPostRequestBody ToMsMailPostRequestBody(this Message message,
        bool saveToSentItems = false)
    {
        return new SendMailPostRequestBody
        {
            Message = message,
            SaveToSentItems = saveToSentItems
        };
    }

    public static Message ToMsMessage(this MailMessage message)
    {
        var messageContent = message.Content;
        var messageHeader = message.Headers;
        return new Message
        {
            Subject = messageContent.Subject,
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = messageContent.HtmlBody
            },
            ToRecipients = messageHeader.Recipients.ToMsRecipients(),
            CcRecipients = messageHeader.CarbonCopy.ToMsRecipients(),
            BccRecipients = messageHeader.BlindCarbonCopy.ToMsRecipients(),
            ReplyTo = messageHeader.ReplyTo.ToMsRecipients(),
            Importance = messageHeader.Importance.ToMsImportance(),
            From = new Recipient
            {
                EmailAddress = messageHeader.From?.ToMsEmailAddress()
            },
            AdditionalData = messageHeader.NonSpecificHandledHeaders as IDictionary<string, object> ??
                             new Dictionary<string, object>()
        };
    }
}
