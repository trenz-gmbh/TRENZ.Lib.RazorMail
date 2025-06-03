namespace TRENZ.Lib.RazorMail.Models;

/// <summary>
/// Specifies the urgency in which the mail should be delivered.
/// </summary>
/// <remarks>
/// <para>
/// This sets both the <c>Importance</c> and <c>X-Priority</c> headers, for
/// broad compatibility.
/// </para>
/// <para>
/// RFC 2156 (https://www.rfc-editor.org/rfc/rfc2156) defines <c>Importance</c>
/// values <c>low</c>, <c>normal</c>, and <c>high</c>. <c>X-Priority</c> is
/// proprietary, and documented at Microsoft:
/// https://learn.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxcmail/2bb19f1b-b35e-4966-b1cb-1afd044e83ab,
/// and takes the values <c>1</c> (highest) through <c>5</c> (lowest).
/// </para>
/// </remarks>
public enum MailImportance
{
    High = 1,
    Normal = 3,
    Low = 5
}

public static class MailImportanceExtensions
{
    public static string ToImportanceHeaderValue(this MailImportance importance)
        => importance switch
        {
            MailImportance.High => "high",
            MailImportance.Low => "low",
            _ => "normal",
        };

    public static string ToXPriorityHeaderValue(this MailImportance importance)
        => importance switch
        {
            MailImportance.High => "2",
            MailImportance.Low => "4",
            _ => "3",
        };
}
