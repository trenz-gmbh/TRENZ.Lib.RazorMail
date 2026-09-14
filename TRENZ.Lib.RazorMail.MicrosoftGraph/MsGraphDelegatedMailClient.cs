using System.Diagnostics;
using System.Text;

using Azure.Identity;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.Messages.Item.Attachments.CreateUploadSession;
using Microsoft.Graph.Users.Item.SendMail;

using TRENZ.Lib.RazorMail.MSGraph.Exceptions;
using TRENZ.Lib.RazorMail.MSGraph.Models;

using Process = System.Diagnostics.Process;

namespace TRENZ.Lib.RazorMail.MSGraph;

public class MsGraphDelegatedMailClient : MsGraphMailClient
{
    private User? _user;

    internal MsGraphDelegatedMailClient(IOptions<MsGraphOptions> accountOptions, ILogger<MsGraphMailClient> logger) :
        base(accountOptions, logger)
    {
    }

    /// <summary>
    /// Method to initialize the graph client used to interact with the Ms Graph API.
    /// This Method is intended to be called by the redirect point of your application specified in the
    /// appsettings as well as in entra id.
    /// The authcode should then be provided as a query parameter.
    /// For more information about this process see here: https://learn.microsoft.com/en-us/graph/auth-v2-user?tabs=http
    ///
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

    /// <summary>
    /// Method which calls the microsoft login flow.
    /// This will be attempted to be opened in a browser.
    /// See https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow for more details.
    /// </summary>
    public void CallMsLoginPage()
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

        Process.Start(new ProcessStartInfo
        {
            FileName = stringBuilder.ToString(),
            UseShellExecute = true
        });
    }


    protected override async Task<UploadSession?> GetUploadSessionForMessage(string fromMail, string postedMessageId,
        CreateUploadSessionPostRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        return (UploadSession?)await DoMailTaskWithChecks<object?>(fromMail, async () =>
        {
            Microsoft.Graph.Me.Messages.Item.Attachments.CreateUploadSession.CreateUploadSessionPostRequestBody
                request =
                    new()
                    {
                        AttachmentItem = requestBody.AttachmentItem
                    };
            return await GraphServiceClient!.Me.Messages[postedMessageId].Attachments
                .CreateUploadSession.PostAsync(request, cancellationToken: cancellationToken);
        });
    }

    protected override async Task<Message?> PostMessageToInbox(string fromMail, Message message,
        CancellationToken cancellationToken)
    {
        return (Message?)await DoMailTaskWithChecks<object?>(fromMail, async () => await GraphServiceClient!.Me.Messages
            .PostAsync(message, cancellationToken: cancellationToken));
    }

    protected override async Task SendPostedMessage(string fromMail, string messageId,
        CancellationToken cancellationToken)
    {
        await DoMailTaskWithChecks(fromMail, async () => await GraphServiceClient!.Me.Messages[messageId].Send
            .PostAsync(cancellationToken: cancellationToken));
    }

    protected override async Task AddSmallAttachmentToExistingMessage(string fromMail, string messageId,
        FileAttachment fileAttachment,
        CancellationToken cancellationToken)
    {
        await DoMailTaskWithChecks(fromMail, async () => await GraphServiceClient!.Me.Messages[messageId].Attachments
            .PostAsync(fileAttachment, cancellationToken: cancellationToken));
    }

    private async Task<object?> DoMailTaskWithChecks<T>(string fromMail, Func<Task<T>> func)
    {
        EnsureFromMailIsUserMail(fromMail);
        return await func.Invoke();
    }

    private async Task DoMailTaskWithChecks(string fromMail, Func<Task> func)
    {
        EnsureFromMailIsUserMail(fromMail);
        await func.Invoke();
    }

    protected override async Task SendMailWithoutAttachments(string fromMail,
        SendMailPostRequestBody sendMailPostRequestBody,
        CancellationToken cancellationToken)
    {
        await DoMailTaskWithChecks(fromMail, async () =>
        {
            Microsoft.Graph.Me.SendMail.SendMailPostRequestBody requestBody = new()
            {
                Message = sendMailPostRequestBody.Message
            };
            await GraphServiceClient!.Me.SendMail
                .PostAsync(requestBody, cancellationToken: cancellationToken);
        });
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
