using Microsoft.Graph.Models;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MSGraph.Extensions;

public static class MailImportanceExtensions
{
    public static Importance ToMSImportance(this MailImportance importance)
    {
        switch (importance)
        {
            case MailImportance.High:
                return Importance.High;
            case MailImportance.Low:
                return Importance.Low;
            case MailImportance.Normal:
                return Importance.Normal;
            default:
                return Importance.Normal;
        }
    }
}