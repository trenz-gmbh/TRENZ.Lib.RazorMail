using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.MSGraph.Models;
using TRENZ.Lib.RazorMail.Services;

namespace TRENZ.Lib.RazorMail.MSGraph;

public class MSGraphMailClient : IMailClient
{
    public MSGraphMailClient(IOptions<AzureAdOptions> accountOptions, ILogger<MSGraphMailClient> logger)
    {

    }


    public MailHeaderCollection DefaultHeaders { get; } = new();

    public Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
