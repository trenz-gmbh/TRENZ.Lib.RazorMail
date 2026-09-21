using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.MicrosoftGraph.Models;

namespace TRENZ.Lib.RazorMail.MicrosoftGraph.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMsGraphMailClient(
        this IServiceCollection services,
        object serviceKey,
        Action<IServiceProvider, MsGraphMailClient>? configureClient = null
    )
    {
        return services.InternalAddMsGraphMailClient(configureClient, serviceKey);
    }

    private static MsGraphMailClient CreateMsGraphService(IServiceProvider serviceProvider,
        Action<IServiceProvider, MsGraphMailClient>? configureClient)
    {
        var azureAdOptions = serviceProvider.GetRequiredService<IOptions<MsGraphOptions>>();
        var msGraphMailLogger = serviceProvider.GetRequiredService<ILogger<MsGraphMailClient>>();
        MsGraphMailClient msGraphMailClient;
        if (azureAdOptions.Value.IsDelegated)
        {
            msGraphMailClient = new DelegatedMsGraphMailClient(azureAdOptions, msGraphMailLogger);
        }
        else
        {
            msGraphMailClient = new DefaultMsGraphMailClient(azureAdOptions, msGraphMailLogger);
        }

        configureClient?.Invoke(serviceProvider, msGraphMailClient);
        return msGraphMailClient;
    }

    private static IServiceCollection InternalAddMsGraphMailClient(
        this IServiceCollection services,
        Action<IServiceProvider, MsGraphMailClient>? configureClient = null,
        object? serviceKey = null
    )
    {
        services.AddOptions<MsGraphOptions>().BindConfiguration(MsGraphOptions.SectionName);

        if (serviceKey is null)
        {
            services.AddSingleton<IMailClient, MsGraphMailClient>(serviceProvider =>
                CreateMsGraphService(serviceProvider, configureClient));
        }
        else
        {
            services.AddKeyedSingleton<IMailClient, MsGraphMailClient>(serviceKey,
                (serviceProvider, _) => CreateMsGraphService(serviceProvider, configureClient));
        }

        return services;
    }
}
