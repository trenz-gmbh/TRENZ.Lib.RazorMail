using Microsoft.Extensions.DependencyInjection;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.SystemNet.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSystemNetRazorMailClient(this IServiceCollection services)
    {
        services.AddOptions<SmtpAccount>().BindConfiguration(SmtpAccount.SectionName);
        services.AddSingleton<IMailClient, SystemNetMailClient>();

        return services;
    }

    public static IServiceCollection AddSystemNetRazorMailClient(this IServiceCollection services, object? key)
    {
        services.AddOptions<SmtpAccount>().BindConfiguration(SmtpAccount.SectionName);
        services.AddKeyedSingleton<IMailClient, SystemNetMailClient>(key);

        return services;
    }
}
