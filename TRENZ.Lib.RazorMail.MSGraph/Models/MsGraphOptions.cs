namespace TRENZ.Lib.RazorMail.MSGraph.Models;

public record MsGraphOptions
{
    public const string SectionName = "MSGraphOptions";

    public required string TenantId { get; init; }

    public required string ClientId { get; init; }

    //fixme uhhhhhh
    public required string ClientSecret { get; init; }

    public required bool SaveToSentItems { get; init; }

    //fixme enum ?
    public string? RedirectUri { get; init; }
    public string[]? Scopes { get; init; }
}
