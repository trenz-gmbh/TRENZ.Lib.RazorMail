using Microsoft.AspNetCore.Mvc;

using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.MicrosoftGraph;
using TRENZ.Lib.RazorMail.MicrosoftGraph.Exceptions;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.SampleWebApi.Helpers;
using TRENZ.Lib.RazorMail.SampleWebApi.Models;

namespace TRENZ.Lib.RazorMail.SampleWebApi.Controllers;

[Route("[controller]/[action]")]
public class MailController(
    IMailRenderer emailRenderer
)
    : ControllerBase
{
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
    public async Task<IActionResult> SendWithMailKit([FromBody] SendSampleMailRequest request,
        [FromKeyedServices("MailKit")] IMailClient client)
    {
        var message = await MakeMessage(request);

        await client.SendAsync(message);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendWithMsGraph([FromBody] SendSampleMailRequestWithOptions request,
        [FromKeyedServices("MsGraph")] IMailClient client)
    {
        var message = await MakeMessage(request);
        if (request.FileSize is not null)
        {
            FileAttachmentHelper.GenerateAndAddDummyFileAttachmentsToMessage(message, request.FileSize.Value,
                request.FileAmount ?? 1);
        }

        if (request.Importance is not null)
        {
            message.Headers.Importance = request.Importance.ToLower() switch
            {
                "low" => MailImportance.Low,
                "high" => MailImportance.High,
                _ => MailImportance.Normal
            };
        }

        try
        {
            await client.SendAsync(message);
            return Ok();
        }
        catch (RazorMailMsGraphException e)
        {
            return Problem(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendWithSystemNet([FromBody] SendSampleMailRequest request,
        [FromKeyedServices("System.Net.Mail")] IMailClient client)
    {
        var message = await MakeMessage(request);

        await client.SendAsync(message);

        return Ok();
    }

    [HttpPost]
    public IActionResult StartMsAuthenticationProcess([FromKeyedServices("MsGraph")] IMailClient client)
    {
        if (client is not MsGraphDelegatedMailClient mailClient)
        {
            return BadRequest("RazorMail MsGraph is not in delegated mode");
        }

        mailClient.CallMsLoginPage();
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
            Headers = new MailHeaderCollection
            {
                From = request.From,
                Recipients = request.To.Select(x => new MailAddress(x))
            }
        };

        return message;
    }
}
