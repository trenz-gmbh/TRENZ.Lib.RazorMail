using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;

using Moq;

using NUnit.Framework;

using TRENZ.Lib.RazorMail.Services;

namespace TRENZ.Lib.RazorMail.Tests;

public class RazorMailRendererTests
{
    [Test]
    public async Task TestWithoutAttachments()
    {
        var viewEngineMock = new Mock<IRazorViewEngine>();
        var viewMock = new Mock<IView>();
        var tempDataProvider = new Mock<ITempDataProvider>();
        var serviceProvider = new Mock<IServiceProvider>();
        var environment = new Mock<IHostEnvironment>();

        const string contentRootPath = "./";
        environment
            .SetupGet(e => e.ContentRootPath)
            .Returns(contentRootPath)
            .Verifiable();

        const string viewName = "SomeView";
        viewEngineMock
            .Setup(v => v.GetView(
                It.Is(null, EqualityComparer<string?>.Default),
                It.Is(viewName, EqualityComparer<string>.Default),
                It.Is(true, EqualityComparer<bool>.Default)
            ))
            .Returns(ViewEngineResult.Found(viewName, viewMock.Object))
            .Verifiable();

        viewMock
            .Setup(v => v.RenderAsync(It.IsAny<ViewContext>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var model = new
        {
            Test = true,
        };

        var renderer = new RazorMailRenderer(
            viewEngineMock.Object,
            tempDataProvider.Object,
            serviceProvider.Object,
            environment.Object
        );

        var content = await renderer.RenderAsync(viewName, model);

        Assert.Multiple(() =>
        {
            Assert.That(content.Subject, Is.Null);
            Assert.That(content.HtmlBody, Is.Empty);
            Assert.That(content.Attachments, Is.Not.Null);
        });

        Assert.That(content.Attachments, Is.Empty);
    }
}
