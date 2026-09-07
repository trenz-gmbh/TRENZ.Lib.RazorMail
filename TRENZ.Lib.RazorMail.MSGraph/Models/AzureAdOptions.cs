namespace TRENZ.Lib.RazorMail.MSGraph.Models;

public record AzureAdOptions
{
    public const string SectionName = "AzureAd";


    public required string Instance { get; init; }
    public required string TenantId { get; init; }
    public required string ClientId { get; init; }

    public required string RedirectUri { get; init; }

    //fixme uhhhhhh
    public required string ClientSecret { get; init; }
}
