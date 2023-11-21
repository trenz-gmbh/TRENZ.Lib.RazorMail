using System.Diagnostics;

using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using TRENZ.Lib.RazorMail.Services;

var builder = Host.CreateApplicationBuilder(args);

var listener = new DiagnosticListener("Microsoft.AspNetCore");
builder.Services.AddSingleton<DiagnosticListener>(listener);
builder.Services.AddSingleton<DiagnosticSource>(listener);

// builder.Services.AddSingleton<DiagnosticSource>(sp => sp.GetRequiredService<DiagnosticListener>());

builder.Services.AddMvcCore()
                .AddRazorViewEngine();

builder.Services.AddTransient<IRazorEmailRenderer, RazorEmailRenderer>();

builder.Services.AddHostedService<Worker>();

using var host = builder.Build();

await host.RunAsync();

class Worker : IHostedService
{
    // public IRazorViewEngine Engine { get; }
    //
    // public Worker(IRazorViewEngine engine)
    // {
    //     Engine = engine;
    // }
    public IRazorEmailRenderer EmailRenderer { get; }
    
    public Worker(IRazorEmailRenderer emailRenderer)
    {
        EmailRenderer = emailRenderer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}
