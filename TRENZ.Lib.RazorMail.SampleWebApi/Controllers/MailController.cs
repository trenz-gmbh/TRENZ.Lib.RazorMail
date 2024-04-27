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
        var message = await MakeMessage(request);

        await client.SendAsync(message);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendWithMailKit([FromBody] SendSampleMailRequest request, [FromKeyedServices("MailKit")] IMailClient client)
    {
        var message = await MakeMessage(request);

        await client.SendAsync(message);

        return Ok();
    }

    private async Task<MailMessage> MakeMessage(SendSampleMailRequest request)
    {
        var model = new SampleModel(request.Salutation);

        const string view = "Sample";
        var renderedMail = await emailRenderer.RenderAsync(view, model);

        var message = new MailMessage
        {
            Content = renderedMail,
            Headers = new()
            {
                From = request.From,
                Recipients = request.To.Select(x => new MailAddress(x)),
            },
        };

        return message;
    }
}
