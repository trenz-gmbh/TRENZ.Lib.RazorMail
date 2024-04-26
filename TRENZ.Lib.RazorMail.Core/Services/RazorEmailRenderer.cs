using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Services;

/// <summary>
/// via https://scottsauber.com/2018/07/07/walkthrough-creating-an-html-email-template-with-razor-and-razor-class-libraries-and-rendering-it-from-a-net-standard-class-library/
/// </summary>
public class RazorEmailRenderer : IRazorEmailRenderer
{
    private IRazorViewEngine _viewEngine;
    private ITempDataProvider _tempDataProvider;
    private IServiceProvider _serviceProvider;
    readonly IWebHostEnvironment _Environment;

    public RazorEmailRenderer(IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider,
        IWebHostEnvironment environment)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
        _Environment = environment;
    }

    public async Task<MailContent> RenderAsync<TModel>(string viewName, TModel model)
    {
        var actionContext = GetActionContext();
        var view = FindView(actionContext, viewName);

        using (var output = new StringWriter())
        {
            var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                { Model = model };

            var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);
            tempData["ContentRootPath"] = this._Environment.ContentRootPath;

            var htmlHelperOptions = new HtmlHelperOptions();

            var viewContext = new ViewContext(actionContext, view, viewData, tempData, output, htmlHelperOptions);

            await view.RenderAsync(viewContext);

            return new MailContent(subject: viewContext.ViewData["Subject"] as string,
                htmlBody: output.ToString(),
                attachments: viewContext.ViewData["Attachments"] as Dictionary<string, MailAttachment>);
        }
    }

    private IView FindView(ActionContext actionContext, string viewName)
    {
        var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        var findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(
                searchedLocations));
        ;

        throw new InvalidOperationException(errorMessage);
    }

    private ActionContext GetActionContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = _serviceProvider;
        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }
}

public interface IRazorEmailRenderer
{
    Task<MailContent> RenderAsync<TModel>(string viewName, TModel model);
}