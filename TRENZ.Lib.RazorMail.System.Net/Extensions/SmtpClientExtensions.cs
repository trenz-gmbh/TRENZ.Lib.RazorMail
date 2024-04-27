using System;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace TRENZ.Lib.RazorMail.Extensions;

public static class SmtpClientExtensions
{
    public static Task SendAsyncWithCancellation(this SmtpClient client, MailMessage message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tcs = new TaskCompletionSource<object?>();
        var sendGuid = Guid.NewGuid();

        cancellationToken.Register(() =>
        {
            if (tcs.Task.IsCanceled || tcs.Task.IsFaulted || tcs.Task.IsCompleted)
                return;

            client.SendAsyncCancel();
        });

        SendCompletedEventHandler? handler = null;

        handler = (_, ea) =>
        {
            if (ea.UserState is not Guid userStateGuid || userStateGuid != sendGuid) return;

            client.SendCompleted -= handler;

            if (ea.Cancelled)
                tcs.SetCanceled(cancellationToken);
            else if (ea.Error != null)
                tcs.SetException(ea.Error);
            else
                tcs.SetResult(null);
        };

        client.SendCompleted += handler;
        client.SendAsync(message, sendGuid);

        return tcs.Task;
    }
}
