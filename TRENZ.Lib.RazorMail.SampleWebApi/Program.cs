using TRENZ.Lib.RazorMail.Extensions;
using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.MailKit.Extensions;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.MSGraph.Extensions;
using TRENZ.Lib.RazorMail.SystemNet.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", true);

builder.Services.AddRazorMailRenderer();
builder.Services.AddMailKitMailClient("MailKit", ConfigureClient);
builder.Services.AddSystemNetMailClient("System.Net.Mail", ConfigureClient);
builder.Services.AddMsGraphMailClient("MsGraph", ConfigureClient);

var app = builder.Build();
// force to build MsGraph service so authorization can be started
var service = app.Services.GetRequiredKeyedService<IMailClient>("MsGraph");

app.MapControllers();

app.Run();

return;

static void ConfigureClient(IServiceProvider sp, IMailClient client)
{
    client.DefaultHeaders.ReplyTo = [new MailAddress("support@example.com")];
}
