
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MSGraph.Extensions;

public static class MailMessageExtensions
{
    public static SendMailPostRequestBody ToMSMailPostRequestBody(this MailMessage message)
    {
        var messageContent = message.Content;
        var msGraphRecipients = new List<Recipient>();
        foreach (var domainRecipients in message.Headers.Recipients)
        {
            msGraphRecipients.Add(
                new Recipient
                {
                    EmailAddress = domainRecipients.ToMSEmailAddress()
                }
            );
        }

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
                ToRecipients = msGraphRecipients
            },
            SaveToSentItems = false
        };
    }



}
