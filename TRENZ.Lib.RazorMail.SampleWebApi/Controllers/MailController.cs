using Microsoft.AspNetCore.Mvc;

using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.SampleWebApi.Models;
using TRENZ.Lib.RazorMail.Services;

namespace TRENZ.Lib.RazorMail.SampleWebApi.Controllers;

[Route("[controller]/[action]")]
public class MailController(
    IRazorEmailRenderer emailRenderer,
    IConfiguration configuration)
    : ControllerBase
{
    private IRazorEmailRenderer EmailRenderer { get; } = emailRenderer;
    private SmtpAccount SmtpAccount { get; } =
        configuration.GetSection("SmtpAccount").Get<SmtpAccount>()!;

    [HttpPost]
    public async Task<IActionResult> SendWithSystemNet([FromBody] SendSampleMailRequest request)
    {
        var renderedMail = await MakeRenderedMail(request);

        var mail = new SystemNetMailSender(
            from: request.From,
            to: [request.To],
            cc: [],
            bcc: [],
            replyTo: [],
            renderedMail: renderedMail
        );

        await mail.SendAsync(SmtpAccount);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendWithMailKit([FromBody] SendSampleMailRequest request)
    {
        var renderedMail = await MakeRenderedMail(request);

        var mail = new MailKitMailSender(
            from: request.From,
            to: [request.To],
            cc: [],
            bcc: [],
            replyTo: [],
            renderedMail: renderedMail
        );

        await mail.SendAsync(SmtpAccount);

        return Ok();
    }

    private async Task<RenderedMail> MakeRenderedMail(SendSampleMailRequest request)
    {
        const string view = "Sample";
        var model = new SampleModel(request.Salutation);
        var renderedMail = await EmailRenderer.RenderAsync(view, model);
        return renderedMail;
    }
}
