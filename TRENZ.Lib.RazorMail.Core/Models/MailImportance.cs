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
