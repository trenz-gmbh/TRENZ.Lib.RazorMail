using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.Messages.Item.Attachments.CreateUploadSession;
using Microsoft.Graph.Users.Item.SendMail;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.MSGraph.Extensions;
using TRENZ.Lib.RazorMail.MSGraph.Models;

namespace TRENZ.Lib.RazorMail.MSGraph;

public abstract class MsGraphMailClient : IMailClient
{
    protected readonly ILogger<MsGraphMailClient> Logger;
    protected readonly MsGraphOptions Options;
    protected GraphServiceClient? GraphServiceClient = null;

    internal MsGraphMailClient(IOptions<MsGraphOptions> accountOptions, ILogger<MsGraphMailClient> logger)
    {
        if (GraphServiceClient is not null)
        {
            return;
        }

        Options = accountOptions.Value;
        Logger = logger;
    }

    public MailHeaderCollection DefaultHeaders { get; } = new();

    public async Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        await SendInternalAsync(message, cancellationToken);
    }


    private async Task SendInternalAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        if (GraphServiceClient is null)
        {
            Logger.LogWarning("A mail was attempted to be send but there is no GraphServiceClient initialized");
            return;
        }

        if (message.Headers.From is null)
        {
            Logger.LogWarning("Sending mail was not possible since the message has no sender defined");
            return;
        }

        var fromMail = message.Headers.From.Email;
        var msMessage = message.ToMsMessage();
        if (message.Content.Attachments.Count > 0)
        {
            await HandleMessageWithAttachments(msMessage, message, fromMail, cancellationToken);
            return;
        }

        await SendMailWithoutAttachments(fromMail, msMessage.ToMsMailPostRequestBody(Options.SaveToSentItems),
            cancellationToken);
        Logger.LogInformation("Sending mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc}) with subject {Subject}",
            fromMail, msMessage.ToRecipients, msMessage.CcRecipients, msMessage.BccRecipients, msMessage.Subject);
    }

    private async Task HandleMessageWithAttachments(Message msMessage, MailMessage message, string fromMail,
        CancellationToken cancellationToken)
    {
        var msFileAttachments = message.Content.Attachments.ToDictionary(
            stringToAttachmentValuePair => stringToAttachmentValuePair.Key,
            stringToAttachmentValuePair => stringToAttachmentValuePair.Value.ToFileAttachment());

        var postedMessage = await PostMessageToInbox(fromMail, msMessage, cancellationToken);
        if (postedMessage == null)
        {
            Logger.LogError(
                "Failed to post message in preparation for attachment upload. For mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc}) with subject: {Subject}",
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
                await AddSmallAttachmentToExistingMessage(fromMail, postedMessage.Id, msFileAttachment.Value,
                    cancellationToken);
                continue;
            }

            if (contentBytesLength > 150e6)
            {
                Logger.LogWarning(
                    "Skipping attachment with name {FileName} because its file size exceeds 150 MB. For mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc}) with subject {Subject}",
                    msFileAttachment.Value.Name, fromMail, msMessage.ToRecipients, msMessage.CcRecipients,
                    msMessage.BccRecipients, msMessage.Subject);
                continue;
            }

            await UploadLargerAttachmentViaSession(fromMail, postedMessage.Id, msFileAttachment.Value,
                cancellationToken);
        }

        await SendPostedMessage(fromMail, postedMessage.Id, cancellationToken);
        Logger.LogInformation("Sending mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc}) with subject {Subject}",
            fromMail, msMessage.ToRecipients, msMessage.CcRecipients, msMessage.BccRecipients, msMessage.Subject);
    }

    private async Task UploadLargerAttachmentViaSession(string fromMail, string postedMessageId,
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
        var uploadSession = await GetUploadSessionForMessage(fromMail, postedMessageId, attachmentUploadRequestBody,
            cancellationToken);
        using var stream = new MemoryStream(fileAttachment.ContentBytes);
        var largeFileUploadTask =
            new LargeFileUploadTask<FileAttachment>(uploadSession, stream);
        await largeFileUploadTask.UploadAsync(cancellationToken: cancellationToken);
    }

    protected abstract Task<UploadSession> GetUploadSessionForMessage(string fromMail, string postedMessageId,
        CreateUploadSessionPostRequestBody requestBody,
        CancellationToken cancellationToken);

    protected abstract Task<Message?>
        PostMessageToInbox(string fromMail, Message message, CancellationToken cancellationToken);

    protected abstract Task SendPostedMessage(string fromMail, string messageId, CancellationToken cancellationToken);

    protected abstract Task AddSmallAttachmentToExistingMessage(string fromMail, string messageId,
        FileAttachment fileAttachment,
        CancellationToken cancellationToken);

    protected abstract Task SendMailWithoutAttachments(string fromMail, SendMailPostRequestBody sendMailPostRequestBody,
        CancellationToken cancellationToken);
}
