using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.SystemNet.Extensions;

public static class MailAddressExtensions
{
    public static global::System.Net.Mail.MailAddress ToMailAddress(this MailAddress address) =>
        new(address.Email, address.Name);
}
