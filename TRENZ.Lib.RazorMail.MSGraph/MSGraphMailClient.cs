using Azure.Identity;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.MSGraph.Extensions;
using TRENZ.Lib.RazorMail.MSGraph.Models;

using SendMailPostRequestBody = Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody;

namespace TRENZ.Lib.RazorMail.MSGraph;

public class MSGraphMailClient : IMailClient
{

    private GraphServiceClient? _graphServiceClient;
    private MSGraphOptions _options;
    private ILogger<MSGraphMailClient> _logger;

    internal MSGraphMailClient(IOptions<MSGraphOptions> accountOptions, ILogger<MSGraphMailClient> logger)
    {
        if (_graphServiceClient is null)
        {
            _options = accountOptions.Value;
            _logger = logger;
            InitalizeGraphClient();
        }
    }

    public MailHeaderCollection DefaultHeaders { get; } = new();

    public Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        return SendInternalAsync(message, cancellationToken);
    }


    private void InitalizeGraphClient()
    {
        var tenantId = _options.TenantId;
        var clientId = _options.ClientId;
        var clientSecret = _options.ClientSecret;

        var options = new ClientSecretCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };
        var clientSecretCredential = new ClientSecretCredential(
            tenantId, clientId, clientSecret, options);

        _graphServiceClient = new GraphServiceClient(clientSecretCredential);
        _logger.Log(LogLevel.Information, "Graph client initialized.");
    }


    private async Task SendInternalAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        if (_graphServiceClient is null)
        {
            //fixme error handling or smth
            return;
        }

        if (message.Headers.From is null)
        {
            //fixme error handling or smth
            return;
        }

        var requestBody = message.ToMSMailPostRequestBody(_options.SaveToSentItems);
        await _graphServiceClient.Users[message.Headers.From.Email].SendMail.PostAsync(requestBody);
    }

}
