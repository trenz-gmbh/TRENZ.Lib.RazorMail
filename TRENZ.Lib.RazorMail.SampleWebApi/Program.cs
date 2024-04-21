using TRENZ.Lib.RazorMail.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);

builder.Services.AddRazorEmailRenderer();

var app = builder.Build();

app.MapControllers();

app.Run();