namespace TRENZ.Lib.RazorMail.MicrosoftGraph.Models;

public record MsGraphOptions
{
    public const string SectionName = "MsGraphOptions";

    public required string ClientId { get; init; }

    public required string ClientSecret { get; init; }

    public required bool IsDelegated { get; init; }

    public string? RedirectUri { get; init; }

    /// <summary>
    /// This setting prevents mails from being saved to sent items if there are no attachments
    /// or if all present attachments are under 3MB.
    /// If any attachment is over 3MB the message needs to created beforehand to upload the attachment to it.
    /// At that stage the api currently has no option to not save it to sent items.
    /// </summary>
    public required bool SaveToSentItems { get; init; }

    public required string TenantId { get; init; }
}
