using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TRENZ.Lib.RazorMail.SystemNetExtensions;

public static class SmtpClientExtensions
{
    public static Task SendAsync(this SmtpClient client, MailMessage message)
    {
        var tcs = new TaskCompletionSource<object?>();
        Guid sendGuid = Guid.NewGuid();

        SendCompletedEventHandler? handler = null;
        
        handler = (_, ea) =>
        {
            if (ea.UserState is not Guid userStateGuid || userStateGuid != sendGuid) return;
            
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
        };

        client.SendCompleted += handler;
        client.SendAsync(message, sendGuid);
        return tcs.Task;
    }
}