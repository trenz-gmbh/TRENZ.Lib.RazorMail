using Microsoft.AspNetCore.Mvc;

using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.SampleWebApi.Models;
using TRENZ.Lib.RazorMail.Services;

namespace TRENZ.Lib.RazorMail.SampleWebApi.Controllers;

public class MailController : ControllerBase
{
    public IRazorEmailRenderer EmailRenderer { get; }
    public SmtpAccount SmtpAccount { get; }

    public MailController(IRazorEmailRenderer emailRenderer,
        IConfiguration configuration)
    {
        EmailRenderer = emailRenderer;
        SmtpAccount = configuration.GetSection("SmtpAccount").Get<SmtpAccount>();
    }

    [HttpPost]
    [Route("[controller]/[action]")]
    public async Task<IActionResult> SendSampleMail([FromBody] SendSampleMailRequest request)
    {
        const string view = "Sample";
        var model = new SampleModel(request.Salutation);
        var renderedMail = await EmailRenderer.RenderAsync(view, model);

        var mail = new MailSender(from: request.From,
            to: new[] { (MailAddress)request.To }.ToList(),
            renderedMail);

        await mail.SendViaSystemNetMailAsync(SmtpAccount);

        return Ok();
    }
}