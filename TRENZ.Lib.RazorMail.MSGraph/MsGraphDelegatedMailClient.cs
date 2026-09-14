using System.Diagnostics;
using System.Text;

using Azure.Identity;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.Messages.Item.Attachments.CreateUploadSession;
using Microsoft.Graph.Users.Item.SendMail;

using TRENZ.Lib.RazorMail.MSGraph.Models;

namespace TRENZ.Lib.RazorMail.MSGraph;

public class MsGraphDelegatedMailClient : MSGraphMailClient
{
    private User _user;
    public MsGraphDelegatedMailClient(IOptions<MSGraphOptions> accountOptions, ILogger<MSGraphMailClient> logger) :
        base(accountOptions, logger)
    {
        CallMSLoginPage();
    }

    public async Task InitializeGraphClientViaAuthCode(string authCode)
    {
        if (GraphServiceClient is not null)
        {
            //log
            return;
        }

        var scopes = new[] { "User.Read", "Mail.ReadWrite", "Mail.Send" };
        var tenantId = Options.TenantId;
        var clientId = Options.ClientId;
        var clientSecret = Options.ClientSecret;
        var options = new AuthorizationCodeCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            RedirectUri =  new Uri(Options.RedirectUri)
        };

        // https://learn.microsoft.com/dotnet/api/azure.identity.authorizationcodecredential
        var authCodeCredential = new AuthorizationCodeCredential(
            tenantId, clientId, clientSecret, authCode, options);

        GraphServiceClient = new GraphServiceClient(authCodeCredential, scopes);
        var me = await GraphServiceClient.Me.GetAsync();
        _user = me;
    }

    public void CallMSLoginPage()
    {
        var scopes = new[] { "User.Read", "Mail.ReadWrite", "Mail.Send" };
        var stringBuilder = new StringBuilder("https://login.microsoftonline.com/");
        stringBuilder.Append(Options.TenantId).Append("/oauth2/v2.0/authorize?client_id=");
        stringBuilder.Append(Options.ClientId).Append("&response_type=code");
        stringBuilder.Append("&redirect_uri=").Append(Options.RedirectUri);
        stringBuilder.Append("&response_mode=query&scope=offline_access");
        foreach (var scope in scopes)
        {
            stringBuilder.Append("%20").Append(scope);
        }

        System.Diagnostics.Process.Start(new ProcessStartInfo()
        {
            FileName = stringBuilder.ToString(),
            UseShellExecute = true
        });
    }


    protected override async Task<UploadSession> GetUploadSessionForMessage(string fromMail, string postedMessageId,
        CreateUploadSessionPostRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        Microsoft.Graph.Me.Messages.Item.Attachments.CreateUploadSession.CreateUploadSessionPostRequestBody request =
            new()
            {
                AttachmentItem = requestBody.AttachmentItem,
            };
        return await GraphServiceClient.Me.Messages[postedMessageId].Attachments
            .CreateUploadSession.PostAsync( request, cancellationToken: cancellationToken);
    }

    protected override async Task<Message?> PostMessageToInbox(string fromMail, Message message,
        CancellationToken cancellationToken)
    {
        return await GraphServiceClient.Me.Messages
            .PostAsync(message, cancellationToken: cancellationToken);
    }

    protected override async Task SendPostedMessage(string fromMail, string messageId,
        CancellationToken cancellationToken)
    {
        await GraphServiceClient.Me.Messages[messageId].Send
            .PostAsync(cancellationToken: cancellationToken);
    }

    protected override async Task AddSmallAttachmentToExistingMessage(string fromMail, string messageId,
        FileAttachment fileAttachment,
        CancellationToken cancellationToken)
    {
        await GraphServiceClient.Me.Messages[messageId].Attachments
            .PostAsync(fileAttachment, cancellationToken: cancellationToken);
    }

    protected override async Task SendMailWithoutAttachments(string fromMail,
        SendMailPostRequestBody sendMailPostRequestBody,
        CancellationToken cancellationToken)
    {
        Microsoft.Graph.Me.SendMail.SendMailPostRequestBody requestBody = new()
        {
            Message = sendMailPostRequestBody.Message,
        };
        await GraphServiceClient.Me.SendMail
            .PostAsync(requestBody, cancellationToken: cancellationToken);
    }
}
