using Microsoft.Extensions.DependencyInjection;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MSGraph.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMSGraphMailClient(
        this IServiceCollection services,
        object serviceKey,
        Action<IServiceProvider, MSGraphMailClient>? configureClient = null
    )
    {
        return services;
    }

    private static IServiceCollection InternalAddMSGraphMailClient(
        this IServiceCollection services,
        Action<IServiceProvider, MSGraphMailClient>? configureClient = null,
        object? serviceKey = null
    )
    {
        services.AddOptions<SmtpAccount>().BindConfiguration(SmtpAccount.SectionName);

        // if (serviceKey is null)
        //     services.AddSingleton<IMailClient, MSGraphMailClient>(sp =>
        //         CreateMailKitMailClient(sp, configureClient));
        // else
        //     services.AddKeyedSingleton<IMailClient, MSGraphMailClient>(serviceKey,
        //         (sp, _) => CreateMailKitMailClient(sp, configureClient));

        return services;
    }
}
