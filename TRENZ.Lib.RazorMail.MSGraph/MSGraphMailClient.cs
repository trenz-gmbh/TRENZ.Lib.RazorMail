using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.Messages.Item.Attachments.CreateUploadSession;
using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.MSGraph.Extensions;
using TRENZ.Lib.RazorMail.MSGraph.Models;

namespace TRENZ.Lib.RazorMail.MSGraph;

public class MSGraphMailClient : IMailClient
{
    private readonly ILogger<MSGraphMailClient> _logger;
    private readonly MSGraphOptions _options;
    private readonly GraphServiceClient _graphServiceClient;

    internal MSGraphMailClient(IOptions<MSGraphOptions> accountOptions, ILogger<MSGraphMailClient> logger)
    {
        if (_graphServiceClient is not null)
        {
            return;
        }

        _options = accountOptions.Value;
        _logger = logger;
        _graphServiceClient = InitalizeGraphClient();
        _logger.Log(LogLevel.Information, "Graph client initialized.");
    }

    public MailHeaderCollection DefaultHeaders { get; } = new();

    public async Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        await SendInternalAsync(message, cancellationToken);
    }


    private GraphServiceClient InitalizeGraphClient()
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
        return new GraphServiceClient(clientSecretCredential);
    }


    private async Task SendInternalAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        if (message.Headers.From is null)
        {
            _logger.LogWarning("Sending mail was not possible since the message has no sender defined");
            return;
        }
        var fromMail = message.Headers.From.Email;
        var msMessage = message.ToMSMessage();
        if (message.Content.Attachments.Count > 0)
        {
            await HandleMessageWithAttachments(msMessage, message, fromMail, cancellationToken);
            return;
        }

        await _graphServiceClient.Users[fromMail].SendMail.PostAsync(
            msMessage.ToMSMailPostRequestBody(_options.SaveToSentItems), cancellationToken: cancellationToken);
        _logger.LogInformation("Sending mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc}) with subject {Subject}",
            fromMail, msMessage.ToRecipients, msMessage.CcRecipients, msMessage.BccRecipients, msMessage.Subject);
    }

    private async Task HandleMessageWithAttachments(Message msMessage, MailMessage message, string fromMail,
        CancellationToken cancellationToken)
    {
        var msFileAttachments = message.Content.Attachments.ToDictionary(
            stringToAttachmentValuePair => stringToAttachmentValuePair.Key,
            stringToAttachmentValuePair => stringToAttachmentValuePair.Value.ToFileAttachment());

        var postedMessage = await _graphServiceClient.Users[fromMail].Messages
            .PostAsync(msMessage, cancellationToken: cancellationToken);
        if (postedMessage == null)
        {
            _logger.LogError(
                "Failed to post message in preparation for attachment upload. For mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc}) with subject {Subject}",
                fromMail, msMessage.ToRecipients, msMessage.CcRecipients, msMessage.BccRecipients, msMessage.Subject);
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
                _logger.LogWarning(
                    "Skipping attachment with name {FileName} because its file size exceeds 150 MB. For mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc}) with subject {Subject}",
                    msFileAttachment.Value.Name, fromMail, msMessage.ToRecipients, msMessage.CcRecipients,
                    msMessage.BccRecipients, msMessage.Subject);
                continue;
            }

            await UploadLargerAttachmentViaSession(fromMail, postedMessage.Id, msFileAttachment.Value,
                cancellationToken);
        }

        await _graphServiceClient.Users[fromMail].Messages[postedMessage.Id].Send
            .PostAsync(cancellationToken: cancellationToken);
        _logger.LogInformation("Sending mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc}) with subject {Subject}",
            fromMail, msMessage.ToRecipients, msMessage.CcRecipients, msMessage.BccRecipients, msMessage.Subject);
    }

    private async Task UploadLargerAttachmentViaSession(string fromMail, string? postedMessageId,
        FileAttachment fileAttachment, CancellationToken cancellationToken)
    {
        var fileSize = fileAttachment.ContentBytes.Length;
        var attachmentUploadRequestBody = new CreateUploadSessionPostRequestBody
        {
            AttachmentItem = new AttachmentItem
            {
                AttachmentType = AttachmentType.File,
                Name = fileAttachment.Name,
                Size = fileSize
            }
        };
        var uploadSession = await _graphServiceClient.Users[fromMail].Messages[postedMessageId].Attachments
            .CreateUploadSession.PostAsync(attachmentUploadRequestBody, cancellationToken: cancellationToken);

        using var stream = new MemoryStream(fileAttachment.ContentBytes);
        var largeFileUploadTask =
            new LargeFileUploadTask<FileAttachment>(uploadSession, stream);
        await largeFileUploadTask.UploadAsync(cancellationToken: cancellationToken);
    }
}
