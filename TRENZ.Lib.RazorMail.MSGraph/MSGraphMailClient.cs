using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.Services;

namespace TRENZ.Lib.RazorMail.MSGraph;

public class MSGraphMailClient(IOptions<SmtpAccount> accountOptions, ILogger<MSGraphMailClient> logger)
    : BaseSmtpMailClient(accountOptions)
{
    //todo client?

    protected override Task SendInternalAsync(MailMessage message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
