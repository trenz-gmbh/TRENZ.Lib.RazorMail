using Azure.Identity;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Sites.Item.TermStore.Sets.Item.Relations.Item.FromTerm;
using Microsoft.Graph.Users.Item.Messages.Item.Attachments.CreateUploadSession;

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
        if (_graphServiceClient is null)
        {
            //fixme error handling or smth
            return Task.CompletedTask;
        }

        if (message.Headers.From is null)
        {
            //fixme error handling or smth
            return Task.CompletedTask;
        }

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
        var fromMail = message.Headers.From.Email;
        var msMessage = message.ToMSMessage();
        if (message.Content.Attachments.Count > 0)
        {
            await HandleMessageWithAttachments(msMessage, message, fromMail, cancellationToken);
            return;
        }

        await _graphServiceClient.Users[fromMail].SendMail.PostAsync(
            msMessage.ToMSMailPostRequestBody(_options.SaveToSentItems), cancellationToken: cancellationToken);
    }

    private async Task HandleMessageWithAttachments(Message msMessage, MailMessage message, string fromMail,
        CancellationToken cancellationToken)
    {
        var msFileAttachments = new Dictionary<string, FileAttachment>();
        foreach (var stringToAttachmentValuePair in message.Content.Attachments)
        {
            msFileAttachments.Add(stringToAttachmentValuePair.Key,
                stringToAttachmentValuePair.Value.ToFileAttachment());
        }

        var postedMessage = await _graphServiceClient.Users[fromMail].Messages
            .PostAsync(msMessage, cancellationToken: cancellationToken);
        if (postedMessage == null)
        {
            //fixme error handling
            return;
        }

        foreach (var msFileAttachment in msFileAttachments)
        {
            /*
             * https://learn.microsoft.com/en-us/graph/outlook-large-attachments
             *
             * If under 3 MB we are supposed to add it directly to the message
             * else we need to create an upload session.
             * It is not clear what exactly 3 MB constitutes for MS therefore we go with SI unit.
             */
            var contentBytesLength = msFileAttachment.Value.ContentBytes.Length;
            if (contentBytesLength < 3e6)
            {
                await _graphServiceClient.Users[fromMail].Messages[postedMessage.Id].Attachments
                    .PostAsync(msFileAttachment.Value, cancellationToken: cancellationToken);
                continue;
            }

            if (contentBytesLength > 150e6)
            {
                //fixme error handling
                return;
            }

            await UploadLargerAttachmentViaSession(fromMail, postedMessage.Id, msFileAttachment.Value,
                cancellationToken);
        }

        await _graphServiceClient.Users[fromMail].Messages[postedMessage.Id].Send.PostAsync(cancellationToken: cancellationToken);
    }

    private async Task UploadLargerAttachmentViaSession(string fromMail, string? postedMessageId,
        FileAttachment fileAttachment, CancellationToken cancellationToken)
    {
        var fileSize = fileAttachment.ContentBytes.Length;
        var attachmentUploadRequestBody = new CreateUploadSessionPostRequestBody()
        {
            AttachmentItem = new AttachmentItem()
            {
                AttachmentType = AttachmentType.File,
                Name = fileAttachment.Name,
                Size = fileSize
            }
        };
        var uploadSession = await _graphServiceClient.Users[fromMail].Messages[postedMessageId].Attachments
            .CreateUploadSession.PostAsync(attachmentUploadRequestBody, cancellationToken: cancellationToken);

        using (var stream = new MemoryStream(fileAttachment.ContentBytes))
        {
            LargeFileUploadTask<FileAttachment> largeFileUploadTask = new LargeFileUploadTask<FileAttachment>(uploadSession, stream);
            await largeFileUploadTask.UploadAsync();
        }
    }
}
