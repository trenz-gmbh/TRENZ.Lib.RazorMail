using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.MSGraph.Models;

namespace TRENZ.Lib.RazorMail.MSGraph.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMSGraphMailClient(
        this IServiceCollection services,
        object serviceKey,
        Action<IServiceProvider, MSGraphMailClient>? configureClient = null
    )
    {
        return services.InternalAddMSGraphMailClient(configureClient, serviceKey);
    }

    private static IServiceCollection InternalAddMSGraphMailClient(
        this IServiceCollection services,
        Action<IServiceProvider, MSGraphMailClient>? configureClient = null,
        object? serviceKey = null
    )
    {
        services.AddOptions<AzureAdOptions>().BindConfiguration(AzureAdOptions.SectionName);

        if (serviceKey is null)
        {
            services.AddSingleton<IMailClient, MSGraphMailClient>(serviceProvider =>
                CreateMSGraphService(serviceProvider, configureClient));
        }
        else
        {
            services.AddKeyedSingleton<IMailClient, MSGraphMailClient>(serviceKey,
                (serviceProvider, _) => CreateMSGraphService(serviceProvider, configureClient));
        }

        return services;
    }

    private static MSGraphMailClient CreateMSGraphService(IServiceProvider serviceProvider,
        Action<IServiceProvider, MSGraphMailClient>? configureClient)
    {
        var azureAdOptions = serviceProvider.GetRequiredService<IOptions<AzureAdOptions>>();
        var msGraphMailLogger = serviceProvider.GetRequiredService<ILogger<MSGraphMailClient>>();

        var msGraphMailClient = new MSGraphMailClient(azureAdOptions, msGraphMailLogger);
        configureClient?.Invoke(serviceProvider, msGraphMailClient);
        return msGraphMailClient;
    }
}
