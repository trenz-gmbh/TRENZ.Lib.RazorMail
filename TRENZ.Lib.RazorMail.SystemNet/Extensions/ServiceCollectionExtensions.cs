using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.SystemNet.Extensions;

public static class ServiceCollectionExtensions
{
    [Obsolete("Use AddSystemNetMailClient instead.")]
    public static IServiceCollection AddSystemNetRazorMailClient(this IServiceCollection services)
        => services.AddSystemNetMailClient();

    public static IServiceCollection AddSystemNetMailClient(
        this IServiceCollection services,
        Action<IServiceProvider, SystemNetMailClient>? configureClient = null
    ) => services.InternalAddSystemNetMailClient(configureClient);


    [Obsolete("Use AddSystemNetMailClient instead.")]
    public static IServiceCollection AddSystemNetRazorMailClient(
        this IServiceCollection services,
        object serviceKey
    ) => services.InternalAddSystemNetMailClient(null, serviceKey);

    public static IServiceCollection AddSystemNetMailClient(
        this IServiceCollection services,
        object serviceKey,
        Action<IServiceProvider, SystemNetMailClient>? configureClient = null
    ) => services.InternalAddSystemNetMailClient(configureClient, serviceKey);

    private static IServiceCollection InternalAddSystemNetMailClient(
        this IServiceCollection services,
        Action<IServiceProvider, SystemNetMailClient>? configureClient = null,
        object? serviceKey = null
    )
    {
        services.AddOptions<SmtpAccount>().BindConfiguration(SmtpAccount.SectionName);

        if (serviceKey is null)
            services.AddSingleton<IMailClient, SystemNetMailClient>(sp =>
                CreateSystemNetMailClient(sp, configureClient));
        else
            services.AddKeyedSingleton<IMailClient, SystemNetMailClient>(serviceKey,
                (sp, _) => CreateSystemNetMailClient(sp, configureClient));

        return services;
    }

    private static SystemNetMailClient CreateSystemNetMailClient(IServiceProvider sp,
        Action<IServiceProvider, SystemNetMailClient>? configureClient)
    {
        var accountOptions = sp.GetRequiredService<IOptions<SmtpAccount>>();
        var logger = sp.GetRequiredService<ILogger<SystemNetMailClient>>();

        var client = new SystemNetMailClient(accountOptions, logger);

        configureClient?.Invoke(sp, client);

        return client;
    }
}
