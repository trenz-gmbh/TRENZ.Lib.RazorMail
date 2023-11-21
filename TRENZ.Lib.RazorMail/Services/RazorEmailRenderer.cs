using System.IO;
using System.Threading;
using System.Threading.Tasks;

using RazorEngineCore;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Services;

public class RazorEmailRenderer : IRazorEmailRenderer
{
    /// <inheritdoc />
    public async Task<RenderedMail> RenderAsync<TModel, TTemplate>(string viewName, TModel model, CancellationToken cancellationToken = default) where TTemplate : RazorEngineTemplateBase<TModel>
    {
        var engine = new RazorEngine();
        var templateSource = await GetTemplateSourceAsync(viewName);
        var template = await engine.CompileAsync<TTemplate>(templateSource, cancellationToken: cancellationToken);
        var result = await template.RunAsync(t =>
        {
            t.Model = model;
        });

        return new RenderedMail(null, result ?? "", null);
    }

    private static Task<string> GetTemplateSourceAsync(string templateName)
    {
        return File.ReadAllTextAsync(templateName);
    }
}