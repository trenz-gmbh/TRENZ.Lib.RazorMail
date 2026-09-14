namespace TRENZ.Lib.RazorMail.MSGraph.Models;

public record MsGraphOptions
{
    public const string SectionName = "MsGraphOptions";

    public required string TenantId { get; init; }

    public required string ClientId { get; init; }

    public required string ClientSecret { get; init; }

    public required bool SaveToSentItems { get; init; }

    public string? RedirectUri { get; init; }
    public required bool Delegated { get; init; }
}
