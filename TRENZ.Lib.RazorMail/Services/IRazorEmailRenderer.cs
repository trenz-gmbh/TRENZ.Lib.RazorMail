using System.Threading;
using System.Threading.Tasks;

using RazorEngineCore;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Services;

public interface IRazorEmailRenderer
{
    Task<RenderedMail> RenderAsync<TModel>(string viewName, TModel model, CancellationToken cancellationToken = default) => RenderAsync<TModel, RazorEngineTemplateBase<TModel>>(viewName, model, cancellationToken);

    Task<RenderedMail> RenderAsync<TModel, TTemplate>(string viewName, TModel model, CancellationToken cancellationToken = default) where TTemplate : RazorEngineTemplateBase<TModel>;
}