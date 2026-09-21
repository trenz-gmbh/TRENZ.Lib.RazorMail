using System.Diagnostics;

using Azure.Identity;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.Messages.Item.Attachments.CreateUploadSession;
using Microsoft.Graph.Users.Item.SendMail;

using TRENZ.Lib.RazorMail.MicrosoftGraph.Exceptions;
using TRENZ.Lib.RazorMail.MicrosoftGraph.Models;

using Process = System.Diagnostics.Process;

namespace TRENZ.Lib.RazorMail.MicrosoftGraph;

public class DelegatedMsGraphMailClient : MsGraphMailClient
{
    private User? _user;

    internal DelegatedMsGraphMailClient(IOptions<MsGraphOptions> accountOptions, ILogger<MsGraphMailClient> logger) :
        base(accountOptions, logger)
    {
    }

    /// <summary>
    ///     Method which calls the microsoft login flow.
    ///     This will be attempted to be opened in a browser.
    ///     See https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow for more details.
    /// </summary>
    public void CallMsLoginPage()
    {
        var scopes = new[] { "User.Read", "Mail.ReadWrite", "Mail.Send", "offline_access" };

        QueryBuilder queryBuilder = new();
        queryBuilder.Add("client_id", Options.ClientId);
        queryBuilder.Add("client_secret", Options.ClientSecret);
        queryBuilder.Add("redirect_uri",  Options.RedirectUri);
        queryBuilder.Add("scope", string.Join(" ", scopes));
        queryBuilder.Add("response_type", "code");
        queryBuilder.Add("response_mode", "query");

        var uriBuilder = new UriBuilder(
            $"https://login.microsoftonline.com/{Options.TenantId}/oauth2/v2.0/authorize")
        {
            Query = queryBuilder.ToString()
        };

        Process.Start(new ProcessStartInfo
        {
            FileName = uriBuilder.ToString(),
            UseShellExecute = true
        });
    }

    /// <summary>
    ///     Method to initialize the graph client used to interact with the Ms Graph API.
    ///     This Method is intended to be called by the redirect point of your application specified in the
    ///     appsettings as well as in entra id.
    ///     The authcode should then be provided as a query parameter.
    ///     For more information about this process see here: https://learn.microsoft.com/en-us/graph/auth-v2-user?tabs=http
    /// </summary>
    /// <param name="authCode">The authorization code provided by the microsoft id platform</param>
    public async Task InitializeGraphClientViaAuthCode(string authCode)
    {
        if (GraphServiceClient is not null)
        {
            Logger.LogInformation("Initializing graph client not possible because it is already initialized.");
            return;
        }

        if (Options.RedirectUri is null)
        {
            Logger.LogError("RedirectUri is not set therefore no authentication can take place");
            return;
        }

        var scopes = new[] { "User.Read", "Mail.ReadWrite", "Mail.Send" };
        var tenantId = Options.TenantId;
        var clientId = Options.ClientId;
        var clientSecret = Options.ClientSecret;
        var options = new AuthorizationCodeCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            RedirectUri = new Uri(Options.RedirectUri)
        };

        // https://learn.microsoft.com/dotnet/api/azure.identity.authorizationcodecredential
        var authCodeCredential = new AuthorizationCodeCredential(
            tenantId, clientId, clientSecret, authCode, options);

        var graphServiceClient = new GraphServiceClient(authCodeCredential, scopes);
        var me = await graphServiceClient.Me.GetAsync();
        if (me is null)
        {
            Logger.LogError("Could not authenticate a user with given settings");
            return;
        }

        _user = me;
        GraphServiceClient = graphServiceClient;
        Logger.LogInformation("Delegated GrapService started for user: {name}", _user.DisplayName);
    }

    protected override async Task AddSmallAttachmentToExistingMessage(string fromMail, string messageId,
        FileAttachment fileAttachment,
        CancellationToken cancellationToken)
    {
        EnsureFromMailIsUserMail(fromMail);
        await GraphServiceClient!.Me.Messages[messageId].Attachments
            .PostAsync(fileAttachment, cancellationToken: cancellationToken);
    }

    protected override async Task<UploadSession?> GetUploadSessionForMessage(string fromMail, string postedMessageId,
        CreateUploadSessionPostRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        EnsureFromMailIsUserMail(fromMail);
        var request =
            new Microsoft.Graph.Me.Messages.Item.Attachments.CreateUploadSession.CreateUploadSessionPostRequestBody
            {
                AttachmentItem = requestBody.AttachmentItem
            };
        return await GraphServiceClient!.Me.Messages[postedMessageId].Attachments
            .CreateUploadSession.PostAsync(request, cancellationToken: cancellationToken);
    }

    protected override async Task<Message?> PostMessageToInbox(string fromMail, Message message,
        CancellationToken cancellationToken)
    {
        EnsureFromMailIsUserMail(fromMail);
        return await GraphServiceClient!.Me.Messages
            .PostAsync(message, cancellationToken: cancellationToken);
    }

    protected override async Task SendMailDirectly(string fromMail,
        SendMailPostRequestBody sendMailPostRequestBody,
        CancellationToken cancellationToken)
    {
        EnsureFromMailIsUserMail(fromMail);
        Microsoft.Graph.Me.SendMail.SendMailPostRequestBody requestBody = new()
        {
            Message = sendMailPostRequestBody.Message
        };
        await GraphServiceClient!.Me.SendMail
            .PostAsync(requestBody, cancellationToken: cancellationToken);
    }

    protected override async Task SendPostedMessage(string fromMail, string messageId,
        CancellationToken cancellationToken)
    {
        EnsureFromMailIsUserMail(fromMail);
        await GraphServiceClient!.Me.Messages[messageId].Send
            .PostAsync(cancellationToken: cancellationToken);
    }

    private void EnsureFromMailIsUserMail(string fromMail)
    {
        if (fromMail.Equals(_user!.Mail))
        {
            return;
        }

        const string errorMessage = "Attempted to send mail from an address that is not the authenticated users";
        Logger.LogWarning(errorMessage);
        throw new RazorMailMsGraphException(errorMessage);
    }
}
