using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

using TRENZ.Lib.RazorMail.Core;
using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Services;

/// <summary>
/// via https://scottsauber.com/2018/07/07/walkthrough-creating-an-html-email-template-with-razor-and-razor-class-libraries-and-rendering-it-from-a-net-standard-class-library/
/// </summary>
public class RazorMailRenderer(
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameterInConstructor",
        Justification = "Only IRazorViewEngine is registered in the DI container.")]
    IRazorViewEngine viewEngine,
    ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider,
    IHostEnvironment environment
)
    : IMailRenderer
{
    public async Task<MailContent> RenderAsync<TModel>(string viewName, TModel model,
        CancellationToken cancellationToken = default)
    {
        var actionContext = GetActionContext();

        var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model,
        };

        var tempData = new TempDataDictionary(actionContext.HttpContext, tempDataProvider)
        {
            [MailTemplateBase<TModel>.ContentRootPathKey] = environment.ContentRootPath,
        };

        var htmlHelperOptions = new HtmlHelperOptions();

        await using var output = new StringWriter();

        var view = FindView(actionContext, viewName);

        var viewContext = new ViewContext(actionContext, view, viewData, tempData, output, htmlHelperOptions);

        cancellationToken.ThrowIfCancellationRequested();

        await view.RenderAsync(viewContext);

        return new()
        {
            Subject = viewContext.ViewData[MailTemplateBase<TModel>.SubjectKey] as string,
            HtmlBody = output.ToString(),
            Attachments =
                (viewContext.ViewData[MailTemplateBase<TModel>.AttachmentsKey] as Dictionary<string, MailAttachment>)!,
        };
    }

    private IView FindView(ActionContext actionContext, string viewName)
    {
        var getViewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
        if (getViewResult.Success)
            return getViewResult.View;

        var findViewResult = viewEngine.FindView(actionContext, viewName, isMainPage: true);
        if (findViewResult.Success)
            return findViewResult.View;

        var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[]
            {
                $"Unable to find view '{viewName}'. The following locations were searched:",
            }.Concat(searchedLocations)
        );

        throw new InvalidOperationException(errorMessage);
    }

    private ActionContext GetActionContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
        };

        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }
}
