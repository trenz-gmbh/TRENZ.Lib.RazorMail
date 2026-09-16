using Microsoft.Graph.Models;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MicrosoftGraph.Extensions;

public static class MailImportanceExtensions
{
    public static Importance ToMsImportance(this MailImportance importance)
    {
        return importance switch
        {
            MailImportance.High => Importance.High,
            MailImportance.Low => Importance.Low,
            _ => Importance.Normal
        };
    }
}
