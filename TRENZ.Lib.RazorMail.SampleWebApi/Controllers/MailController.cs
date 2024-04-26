using Microsoft.AspNetCore.Mvc;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.SampleWebApi.Models;

namespace TRENZ.Lib.RazorMail.SampleWebApi.Controllers;

[Route("[controller]/[action]")]
public class MailController(
    IMailRenderer emailRenderer
)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendWithSystemNet([FromBody] SendSampleMailRequest request, [FromKeyedServices("System.Net.Mail")] IMailClient client)
    {
        var renderedMail = await MakeRenderedMail(request);

        var message = new MailMessage
        {
            Content = renderedMail,
            Headers = new()
            {
                From = request.From,
                Recipients = request.To.Select(x => new MailAddress(x)),
            },
        };

        await client.SendAsync(message);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendWithMailKit([FromBody] SendSampleMailRequest request, [FromKeyedServices("MailKit")] IMailClient client)
    {
        var renderedMail = await MakeRenderedMail(request);

        var message = new MailMessage
        {
            Content = renderedMail,
            Headers = new()
            {
                From = request.From,
                Recipients = request.To.Select(x => new MailAddress(x)),
            },
        };

        await client.SendAsync(message);

        return Ok();
    }

    private async Task<MailContent> MakeRenderedMail(SendSampleMailRequest request)
    {
        const string view = "Sample";
        var model = new SampleModel(request.Salutation);
        var renderedMail = await emailRenderer.RenderAsync(view, model);
        return renderedMail;
    }
}
