namespace TRENZ.Lib.RazorMail.MSGraph.Models;

public record MSGraphOptions
{
    public const string SectionName = "MSGraphOptions";

    public required string TenantId { get; init; }

    public required string ClientId { get; init; }

    //fixme uhhhhhh
    public required string ClientSecret { get; init; }
    public required bool SaveToSentItems { get; init; }
}
