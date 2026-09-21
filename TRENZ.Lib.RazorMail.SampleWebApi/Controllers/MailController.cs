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
    /// <summary>
    ///     This endpoint is relevant for the use of MsGraph with delegated permissions.
    ///     It should be called as a redirect during the Microsoft authentication process.
    ///     As such it should match the redirect uri given in the app registration as well as in the app settings.
    /// </summary>
    /// <param name="authcode">the authentication code provided by Microsoft</param>
    /// <param name="client"> the <see cref="DelegatedMsGraphMailClient" />  </param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> AuthcodeReceiver([FromQuery(Name = "code")] string authcode,
        [FromKeyedServices("MsGraph")] IMailClient client)
    {
        if (client is not DelegatedMsGraphMailClient mailClient)
        {
            return BadRequest("RazorMail MsGraph is not in delegated mode");
        }

        await mailClient.InitializeGraphClientViaAuthCode(authcode);
        return Ok();
    }

    /// <summary>
    ///     Example endpoint to send mails via MailKit.
    /// </summary>
    /// <param name="request"> a <see cref="SendSampleMailRequest" /> </param>
    /// <param name="client"> the <see cref="IMailClient" />  </param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> SendWithMailKit([FromBody] SendSampleMailRequest request,
        [FromKeyedServices("MailKit")] IMailClient client)
    {
        var message = await MakeMessage(request);

        await client.SendAsync(message);

        return Ok();
    }

    /// <summary>
    ///     Example endpoint to send mails via MsGraph.
    /// </summary>
    /// <param name="request"> a <see cref="SendSampleMailRequestWithOptions" /></param>
    /// <param name="client"> the <see cref="IMailClient" />  </param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> SendWithMsGraph([FromBody] SendSampleMailRequestWithOptions request,
        [FromKeyedServices("MsGraph")] IMailClient client)
    {
        var message = await MakeMessage(request);
        if (request.PerFileSizeMb is not null)
        {
            FileAttachmentHelper.GenerateAndAddDummyFileAttachmentsToMessage(message, request.PerFileSizeMb.Value,
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

    /// <summary>
    ///     Example endpoint to send mails via SystemNet.
    /// </summary>
    /// <param name="request"> a <see cref="SendSampleMailRequest" /> </param>
    /// <param name="client"> the <see cref="IMailClient" />  </param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> SendWithSystemNet([FromBody] SendSampleMailRequest request,
        [FromKeyedServices("System.Net.Mail")] IMailClient client)
    {
        var message = await MakeMessage(request);

        await client.SendAsync(message);

        return Ok();
    }

    /// <summary>
    ///     This endpoint is relevant for the use of MsGraph with delegated permissions.
    ///     It should be called (once) to start the Microsoft authentication process.
    ///     This will open a browser.
    /// </summary>
    /// <param name="client"> the <see cref="DelegatedMsGraphMailClient" />  </param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult StartMsAuthenticationProcess([FromKeyedServices("MsGraph")] IMailClient client)
    {
        if (client is not DelegatedMsGraphMailClient mailClient)
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
