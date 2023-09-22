using System;
using System.Threading.Tasks;

namespace EL.Lib.RazorMail.SystemNetMailExtensions;

public static class SmtpClientExtensions
{
    public static Task SendAsync(this System.Net.Mail.SmtpClient client, System.Net.Mail.MailMessage message)
    {
        var tcs = new TaskCompletionSource<object?>();
        Guid sendGuid = Guid.NewGuid();

        System.Net.Mail.SendCompletedEventHandler? handler = null;
        handler = (o, ea) =>
        {
            if (ea.UserState is Guid userStateGuid && userStateGuid == sendGuid)
            {
                client.SendCompleted -= handler;
                if (ea.Cancelled)
                {
                    tcs.SetCanceled();
                }
                else if (ea.Error != null)
                {
                    tcs.SetException(ea.Error);
                }
                else
                {
                    tcs.SetResult(null);
                }
            }
        };

        client.SendCompleted += handler;
        client.SendAsync(message, sendGuid);
        return tcs.Task;
    }
}
