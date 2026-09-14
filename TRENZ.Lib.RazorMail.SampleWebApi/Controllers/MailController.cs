using Microsoft.AspNetCore.Mvc;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.MSGraph;
using TRENZ.Lib.RazorMail.SampleWebApi.Models;

namespace TRENZ.Lib.RazorMail.SampleWebApi.Controllers;

[Route("[controller]/[action]")]
public class MailController(
    IMailRenderer emailRenderer
)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendWithSystemNet([FromBody] SendSampleMailRequest request,
        [FromKeyedServices("System.Net.Mail")] IMailClient client)
    {
        var message = await MakeMessage(request);

        await client.SendAsync(message);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendWithMailKit([FromBody] SendSampleMailRequest request,
        [FromKeyedServices("MailKit")] IMailClient client)
    {
        var message = await MakeMessage(request);

        await client.SendAsync(message);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendWithMsGraph([FromBody] SendSampleMailRequest request,
        [FromKeyedServices("MsGraph")] IMailClient client)
    {
        var message = await MakeMessage(request);

        await client.SendAsync(message);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> AuthcodeReceiver([FromQuery(Name = "code")] string authcode,
        [FromKeyedServices("MsGraph")] IMailClient client)
    {
        if (client is not MsGraphDelegatedMailClient mailClient)
        {
            return BadRequest("RazorMail MsGraph is not in delegated mode");
        }

        await mailClient.InitializeGraphClientViaAuthCode(authcode);
        return Ok();
    }

    [HttpPost]
    public Task<IActionResult> StartMsAuthenticationProcess([FromKeyedServices("MsGraph")] IMailClient client)
    {
        try
        {
            if (client is not MsGraphDelegatedMailClient mailClient)
            {
                return Task.FromResult<IActionResult>(BadRequest("RazorMail MsGraph is not in delegated mode"));
            }
            mailClient.CallMsLoginPage();
            return Task.FromResult<IActionResult>(Ok());
        }
        catch (Exception exception)
        {
            return Task.FromException<IActionResult>(exception);
        }
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
