using Microsoft.Graph.Models;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MSGraph.Extensions;

public static class MailAddressExtensions
{
    public static EmailAddress ToMSEmailAddress(this MailAddress address)
    {
        return new EmailAddress
        {
            Address = address.Email
        };
    }
}
