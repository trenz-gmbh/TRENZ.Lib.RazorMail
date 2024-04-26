using Microsoft.Extensions.DependencyInjection;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Services;

namespace TRENZ.Lib.RazorMail.Extensions;

public static class RazorMailServiceCollectionExtensions
{
    public static IServiceCollection AddRazorEmailRenderer(this IServiceCollection services)
    {
        services.AddMvcCore()
                .AddRazorViewEngine();

        services.AddTransient<IMailRenderer, MailRenderer>();

        return services;
    }
}