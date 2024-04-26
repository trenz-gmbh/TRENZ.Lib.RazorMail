using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.SystemNetExtensions;

public static class MailAddressExtensions
{
    public static System.Net.Mail.MailAddress ToMailAddress(this MailAddress address)
        => new System.Net.Mail.MailAddress(address.Email, address.Name);
}