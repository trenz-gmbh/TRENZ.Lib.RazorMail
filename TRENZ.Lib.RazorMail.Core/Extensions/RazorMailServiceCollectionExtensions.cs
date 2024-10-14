using System;

using Microsoft.Extensions.DependencyInjection;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Services;

namespace TRENZ.Lib.RazorMail.Extensions;

public static class RazorMailServiceCollectionExtensions
{
    [Obsolete("Use AddRazorMailRenderer instead.")]
    public static IServiceCollection AddRazorEmailRenderer(this IServiceCollection services) =>
        services.AddRazorMailRenderer();

    public static IServiceCollection AddRazorMailRenderer(this IServiceCollection services)
    {
        services.AddMvcCore()
            .AddRazorViewEngine();

        services.AddTransient<IMailRenderer, RazorMailRenderer>();

        return services;
    }
}
