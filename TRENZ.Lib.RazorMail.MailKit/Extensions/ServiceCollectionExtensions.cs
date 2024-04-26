using Microsoft.Extensions.DependencyInjection;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMailKitRazorMailClient(this IServiceCollection services)
    {
        services.AddOptions<SmtpAccount>().BindConfiguration(SmtpAccount.SectionName);
        services.AddSingleton<IMailClient, MailKitMailClient>();

        return services;
    }

    public static IServiceCollection AddMailKitRazorMailClient(this IServiceCollection services, object? key)
    {
        services.AddOptions<SmtpAccount>().BindConfiguration(SmtpAccount.SectionName);
        services.AddKeyedSingleton<IMailClient, MailKitMailClient>(key);

        return services;
    }
}
