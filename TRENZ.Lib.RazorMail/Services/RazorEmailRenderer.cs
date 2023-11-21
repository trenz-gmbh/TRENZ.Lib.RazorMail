using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Services;

public class RazorEmailRenderer : IRazorEmailRenderer
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILoggerFactory loggerFactory;

    public RazorEmailRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        this.serviceProvider = serviceProvider;
        this.loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public async Task<RenderedMail> RenderAsync<TComponent, TModel>(string viewName, TModel model) where TComponent : IComponent
    {
        await using var htmlRenderer = new HtmlRenderer(serviceProvider, loggerFactory);

        // var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        // {
        //     var output = await htmlRenderer.RenderComponentAsync<TModel>(ParameterView.Empty);
        //
        //     return output;
        // });

        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var dictionary = new Dictionary<string, object?>
            {
                { "Message", "Hello from the Render Message component!" }
            };

            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await htmlRenderer.RenderComponentAsync<TComponent>(parameters);

            return output.ToHtmlString();
        });

        throw new System.NotImplementedException();
    }
}