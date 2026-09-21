using Microsoft.Graph.Models;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MicrosoftGraph.Extensions;

public static class MailAddressExtensions
{
    public static EmailAddress ToMsEmailAddress(this MailAddress address)
    {
        return new EmailAddress
        {
            Name = address.Name,
            Address = address.Email
        };
    }

    public static List<Recipient> ToMsRecipients(this IEnumerable<MailAddress> addresses)
    {
        return
        [
            .. addresses.Select(mailAdress => new Recipient()
            {
                EmailAddress = mailAdress.ToMsEmailAddress()
            })
        ];
    }
}
