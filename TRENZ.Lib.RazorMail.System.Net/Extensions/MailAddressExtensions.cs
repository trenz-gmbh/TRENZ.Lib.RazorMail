using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Extensions;

public static class MailAddressExtensions
{
    public static System.Net.Mail.MailAddress ToMailAddress(this MailAddress address) =>
        new(address.Email, address.Name);
}
