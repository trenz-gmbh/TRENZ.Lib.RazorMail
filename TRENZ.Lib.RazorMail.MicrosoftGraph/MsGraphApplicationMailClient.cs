using Azure.Identity;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.Messages.Item.Attachments.CreateUploadSession;
using Microsoft.Graph.Users.Item.SendMail;

using TRENZ.Lib.RazorMail.MicrosoftGraph.Models;

namespace TRENZ.Lib.RazorMail.MicrosoftGraph;

public class MsGraphApplicationMailClient : MsGraphMailClient
{
    internal MsGraphApplicationMailClient(IOptions<MsGraphOptions> accountOptions, ILogger<MsGraphMailClient> logger) :
        base(accountOptions, logger)
    {
        InitializeGraphClient();
    }

    protected override async Task AddSmallAttachmentToExistingMessage(string fromMail, string messageId,
        FileAttachment fileAttachment,
        CancellationToken cancellationToken)
    {
        await GraphServiceClient!.Users[fromMail].Messages[messageId].Attachments
            .PostAsync(fileAttachment, cancellationToken: cancellationToken);
    }

    protected override async Task<UploadSession?> GetUploadSessionForMessage(string fromMail, string postedMessageId,
        CreateUploadSessionPostRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        return await GraphServiceClient!.Users[fromMail].Messages[postedMessageId].Attachments
            .CreateUploadSession.PostAsync(requestBody, cancellationToken: cancellationToken);
    }

    protected override async Task<Message?> PostMessageToInbox(string fromMail, Message message,
        CancellationToken cancellationToken)
    {
        return await GraphServiceClient!.Users[fromMail].Messages
            .PostAsync(message, cancellationToken: cancellationToken);
    }

    protected override async Task SendMailDirectly(string fromMail,
        SendMailPostRequestBody sendMailPostRequestBody,
        CancellationToken cancellationToken)
    {
        await GraphServiceClient!.Users[fromMail].SendMail
            .PostAsync(sendMailPostRequestBody, cancellationToken: cancellationToken);
    }

    protected override async Task SendPostedMessage(string fromMail, string messageId,
        CancellationToken cancellationToken)
    {
        await GraphServiceClient!.Users[fromMail].Messages[messageId].Send
            .PostAsync(cancellationToken: cancellationToken);
    }

    private void InitializeGraphClient()
    {
        var tenantId = Options.TenantId;
        var clientId = Options.ClientId;
        var clientSecret = Options.ClientSecret;

        var options = new ClientSecretCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };
        var clientSecretCredential = new ClientSecretCredential(
            tenantId, clientId, clientSecret, options);
        GraphServiceClient = new GraphServiceClient(clientSecretCredential);
    }
}
