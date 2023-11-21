using Microsoft.Extensions.DependencyInjection;

using TRENZ.Lib.RazorMail.Services;

namespace TRENZ.Lib.RazorMail.Extensions;

public static class RazorMailServiceCollectionExtensions
{
    public static IServiceCollection AddRazorEmailRenderer(this IServiceCollection services)
    {
        services.AddSingleton<IRazorEmailRenderer, RazorEmailRenderer>();

        return services;
    }
}