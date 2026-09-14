using TRENZ.Lib.RazorMail.Extensions;
using TRENZ.Lib.RazorMail.Interfaces;
using TRENZ.Lib.RazorMail.MailKit.Extensions;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.MSGraph;
using TRENZ.Lib.RazorMail.MSGraph.Extensions;
using TRENZ.Lib.RazorMail.SystemNet.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);

builder.Services.AddRazorMailRenderer();
builder.Services.AddMailKitMailClient("MailKit", ConfigureClient);
builder.Services.AddSystemNetMailClient("System.Net.Mail", ConfigureClient);
builder.Services.AddMSGraphMailClient("MSGraph", ConfigureClient);

var app = builder.Build();
var service = app.Services.GetRequiredKeyedService<IMailClient>("MSGraph");

app.MapControllers();

app.Run();

return;

static void ConfigureClient(IServiceProvider sp, IMailClient client)
{
    client.DefaultHeaders.ReplyTo = [new MailAddress("support@example.com")];
}
