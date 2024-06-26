using MimeKit;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MailKit.Extensions;

public static class MailAddressExtensions
{
    public static MailboxAddress ToMailboxAddress(this MailAddress mailAddress)
        => new MailboxAddress(mailAddress.Name, mailAddress.Email);
}
