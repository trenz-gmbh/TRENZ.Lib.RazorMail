using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.MailKit.Extensions;

public static class ServiceCollectionExtensions
{
    [Obsolete("Use AddMailKitMailClient instead.")]
    public static IServiceCollection AddMailKitRazorMailClient(this IServiceCollection services) =>
        services.AddMailKitMailClient();

    public static IServiceCollection AddMailKitMailClient(
        this IServiceCollection services,
        Action<IServiceProvider, MailKitMailClient>? configureClient = null
    ) => services.InternalAddMailKitMailClient(configureClient);

    [Obsolete("Use AddMailKitMailClient instead.")]
    public static IServiceCollection AddMailKitRazorMailClient(
        this IServiceCollection services,
        object serviceKey
    ) => services.InternalAddMailKitMailClient(null, serviceKey);

    public static IServiceCollection AddMailKitMailClient(
        this IServiceCollection services,
        object serviceKey,
        Action<IServiceProvider, MailKitMailClient>? configureClient = null
    ) => services.InternalAddMailKitMailClient(configureClient, serviceKey);

    private static IServiceCollection InternalAddMailKitMailClient(
        this IServiceCollection services,
        Action<IServiceProvider, MailKitMailClient>? configureClient = null,
        object? serviceKey = null
    )
    {
        services.AddOptions<SmtpAccount>().BindConfiguration(SmtpAccount.SectionName);

        if (serviceKey is null)
            services.AddSingleton<IMailClient, MailKitMailClient>(sp =>
                CreateMailKitMailClient(sp, configureClient));
        else
            services.AddKeyedSingleton<IMailClient, MailKitMailClient>(serviceKey,
                (sp, _) => CreateMailKitMailClient(sp, configureClient));

        return services;
    }

    private static MailKitMailClient CreateMailKitMailClient(IServiceProvider sp,
        Action<IServiceProvider, MailKitMailClient>? configureClient)
    {
        var accountOptions = sp.GetRequiredService<IOptions<SmtpAccount>>();
        var logger = sp.GetRequiredService<ILogger<MailKitMailClient>>();

        var client = new MailKitMailClient(accountOptions, logger);

        configureClient?.Invoke(sp, client);

        return client;
    }
}
