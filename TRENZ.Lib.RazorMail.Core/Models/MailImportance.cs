namespace TRENZ.Lib.RazorMail.Models;

/// <summary>
/// Specifies the urgency in which the mail should be delivered.
/// </summary>
/// <remarks>
/// This sets both the <c>Importance</c> and <c>X-Priority</c> headers, for
/// broad compatibility.
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