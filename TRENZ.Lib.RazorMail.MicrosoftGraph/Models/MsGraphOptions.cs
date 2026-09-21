namespace TRENZ.Lib.RazorMail.MicrosoftGraph.Models;

public record MsGraphOptions
{
    public const string SectionName = "MsGraphOptions";

    /// <summary>
    ///     The client id is the "Application (client) ID" value in the entra app registration.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    ///     The secret created in the entra app registration.
    /// </summary>
    public required string ClientSecret { get; init; }

    /// <summary>
    ///     This value decides if a mail client which assumes application level permissions or one which assumes delegated
    ///     permissions shall be created.
    ///     This should match the permissions set in the entra app registration.
    /// </summary>
    public required bool IsDelegated { get; init; }

    /// <summary>
    ///     The redirect uri which should match the redirect uri in the entra app registration.
    ///     Only needed in delegated mode to redirect the browser back to the app with the authentication code.
    /// </summary>
    public string? RedirectUri { get; init; }

    /// <summary>
    ///     This setting prevents mails from being saved to sent items if there are no attachments
    ///     or if all present attachments are under 3MB.
    ///     If any attachment is over 3MB the message needs to created beforehand to upload the attachment to it.
    ///     At that stage the api currently has no option to not save it to sent items.
    ///     If the option <see cref="UseUploadSessions"/> is set to false this option would work with attachment sizes greater than 3MB.
    /// </summary>
    public required bool SaveToSentItems { get; init; }

    /// <summary>
    ///     The tenant id is the "Directory (tenant) ID" in the entra app registration.
    ///     If personal accounts should be able to use this application it should be changed to "common".
    /// </summary>
    public required string TenantId { get; init; }

    /// <summary>
    ///     This gives the option to always upload attachments directly to the mail, regardless of size.
    ///     Disabling this is not recommended.
    /// </summary>
    public required bool UseUploadSessions { get; init; }
}
