using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Services;

public interface IRazorEmailRenderer
{
    Task<RenderedMail> RenderAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent, TModel>(string viewName, TModel model)
        where TComponent : IComponent;
}