using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.Messages.Item.Attachments.CreateUploadSession;
using Microsoft.Graph.Users.Item.SendMail;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.MicrosoftGraph.Exceptions;
using TRENZ.Lib.RazorMail.MicrosoftGraph.Extensions;
using TRENZ.Lib.RazorMail.MicrosoftGraph.Models;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MicrosoftGraph;

public abstract class MsGraphMailClient : IMailClient
{
    //these limits from testing seem somewhat arbitrary
    //but these are the values given via microsoft:
    //(09.2026)  https://learn.microsoft.com/en-us/graph/outlook-large-attachments
    private const double MaxSizeAttachmentsMbWithoutUploadSession = 30e6;
    private const double MaxSizeAttachment = 150e6;

    /// <inheritdoc />
    public MailHeaderCollection DefaultHeaders { get; } = new();

    protected readonly ILogger<MsGraphMailClient> Logger;
    protected readonly MsGraphOptions Options;
    protected GraphServiceClient? GraphServiceClient = null;

    internal MsGraphMailClient(IOptions<MsGraphOptions> accountOptions, ILogger<MsGraphMailClient> logger)
    {
        Options = accountOptions.Value;
        Logger = logger;
    }

    /// <inheritdoc />
    [MemberNotNull(nameof(GraphServiceClient))]
    public async Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        await SendInternalAsync(message, cancellationToken);
    }

    private async Task SendInternalAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        if (GraphServiceClient is null)
        {
            Logger.LogWarning("A mail was attempted to be sent but there is no GraphServiceClient initialized");
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
            if (message.Content.Attachments.Values.Any(mailAttachment =>
                    mailAttachment.FileData.Length > MaxSizeAttachmentsMbWithoutUploadSession) &&
                Options.UseUploadSessions)
            {
                /*
                 * https://learn.microsoft.com/en-us/graph/outlook-large-attachments
                 *
                 * If an individual attachment is under 3 MB we are supposed to add it directly to the message
                 * else we need to create an upload session.
                 * It is not clear what exactly 3 MB constitutes for MS therefore we go with SI unit.
                 */
                await HandleMessageWithLargerAttachments(msMessage, message, fromMail, cancellationToken);
                return;
            }

            await HandleMessageWithOnlySmallAttachments(msMessage, message, fromMail, cancellationToken);
            return;
        }

        await ExecuteGraphApiCall(async () => await SendMailDirectly(fromMail,
            msMessage.ToMsMailPostRequestBody(Options.SaveToSentItems),
            cancellationToken));
        LogMailSpecificMessage("Mail successfully sent", msMessage, fromMail, LogLevel.Information);
    }

    private async Task HandleMessageWithOnlySmallAttachments(Message msMessage, MailMessage message, string fromMail,
        CancellationToken cancellationToken)
    {
        List<Attachment> msFileAttachments =
        [
            .. message.Content.Attachments.Values.Select(attachment => attachment.ToMsFileAttachment())
        ];
        msMessage.Attachments = msFileAttachments;
        await ExecuteGraphApiCall(async () => await SendMailDirectly(fromMail,
            msMessage.ToMsMailPostRequestBody(Options.SaveToSentItems), cancellationToken));
        LogMailSpecificMessage("Mail successfully sent", msMessage, fromMail, LogLevel.Information);
    }

    private async Task HandleMessageWithLargerAttachments(Message msMessage, MailMessage message, string fromMail,
        CancellationToken cancellationToken)
    {
        var msFileAttachments = message.Content.Attachments.ToDictionary(
            stringToAttachmentValuePair => stringToAttachmentValuePair.Key,
            stringToAttachmentValuePair => stringToAttachmentValuePair.Value.ToMsFileAttachment());

        var postedMessage = await PostMessageToInbox(fromMail, msMessage, cancellationToken);
        if (postedMessage?.Id is null)
        {
            LogMailSpecificMessage("Failed to post message in preparation for attachment upload.", msMessage, fromMail,
                LogLevel.Error);
            return;
        }

        foreach (var msFileAttachment in msFileAttachments)
        {
            var contentBytesLength = msFileAttachment.Value.ContentBytes!.Length;

            if (contentBytesLength < MaxSizeAttachmentsMbWithoutUploadSession)
            {
                await ExecuteGraphApiCall(async () => await AddSmallAttachmentToExistingMessage(fromMail,
                    postedMessage.Id, msFileAttachment.Value,
                    cancellationToken));
                continue;
            }

            if (contentBytesLength > MaxSizeAttachment)
            {
                LogMailSpecificMessage(
                    $"Skipping attachment with name {msFileAttachment.Value.Name} because its file size exceeds 150 MB",
                    msMessage, fromMail, LogLevel.Warning);
                continue;
            }

            await UploadLargerAttachmentViaSession(fromMail, postedMessage.Id, msFileAttachment.Value,
                cancellationToken);
        }

        await ExecuteGraphApiCall(async () => await SendPostedMessage(fromMail, postedMessage.Id, cancellationToken));
        LogMailSpecificMessage("Mail successfully sent", msMessage, fromMail, LogLevel.Information);
    }

    private async Task UploadLargerAttachmentViaSession(string fromMail, string postedMessageId,
        FileAttachment fileAttachment, CancellationToken cancellationToken)
    {
        var fileSize = fileAttachment.ContentBytes!.Length;
        var attachmentUploadRequestBody = new CreateUploadSessionPostRequestBody
        {
            AttachmentItem = new AttachmentItem
            {
                AttachmentType = AttachmentType.File,
                Name = fileAttachment.Name,
                Size = fileSize
            }
        };
        var uploadSession = await ExecuteGraphApiCall(async () => await GetUploadSessionForMessage(fromMail,
            postedMessageId,
            attachmentUploadRequestBody,
            cancellationToken));
        using var stream = new MemoryStream(fileAttachment.ContentBytes);
        var largeFileUploadTask =
            new LargeFileUploadTask<FileAttachment>(uploadSession, stream);
        await ExecuteGraphApiCall(async () =>
            await largeFileUploadTask.UploadAsync(cancellationToken: cancellationToken));
    }

    private async Task ExecuteGraphApiCall(Func<Task> asyncApiCall)
    {
        try
        {
            await asyncApiCall.Invoke();
        }
        catch (Exception e)
        {
            HandleExceptionDuringApiCall(e);
        }
    }

    private void HandleExceptionDuringApiCall(Exception exception)
    {
        var message = "A exception occured during a call to the Graph Api:\n";
        message += exception switch
        {
            ServiceException serviceException => serviceException.RawResponseBody,
            TaskCanceledException taskCanceledException => taskCanceledException.Message,
            _ => exception.Message
        };

        Logger.LogError("{message}", message);
        throw new RazorMailMsGraphException(message);
    }

    private async Task<T> ExecuteGraphApiCall<T>(Func<Task<T>> asyncApiCall)
    {
        try
        {
            return await asyncApiCall.Invoke();
        }
        catch (Exception e)
        {
            HandleExceptionDuringApiCall(e);
            //unreachable
            return default;
        }
    }

    private void LogMailSpecificMessage(string message, Message msMessage, string fromMail, LogLevel logLevel)
    {
        var recipients = string.Join(" , ",
            msMessage.ToRecipients!.Select(recipient => recipient.EmailAddress!.Address));
        var ccRecipients = string.Join(" , ",
            msMessage.CcRecipients!.Select(recipient => recipient.EmailAddress!.Address));
        var bccRecipients = string.Join(" , ",
            msMessage.BccRecipients!.Select(recipient => recipient.EmailAddress!.Address));
        var messageSuffix =
            $"For mail from {fromMail} to {recipients} (CC: {ccRecipients}, BCC: {bccRecipients}) with subject: \"{msMessage.Subject}\"";
        Logger.Log(logLevel, "{message}\n{messageSuffix}", message, messageSuffix);
    }

    protected abstract Task<UploadSession?> GetUploadSessionForMessage(string fromMail, string postedMessageId,
        CreateUploadSessionPostRequestBody requestBody,
        CancellationToken cancellationToken);

    protected abstract Task<Message?>
        PostMessageToInbox(string fromMail, Message message, CancellationToken cancellationToken);

    protected abstract Task SendPostedMessage(string fromMail, string messageId, CancellationToken cancellationToken);

    protected abstract Task AddSmallAttachmentToExistingMessage(string fromMail, string messageId,
        FileAttachment fileAttachment,
        CancellationToken cancellationToken);

    protected abstract Task SendMailDirectly(string fromMail, SendMailPostRequestBody sendMailPostRequestBody,
        CancellationToken cancellationToken);
}
