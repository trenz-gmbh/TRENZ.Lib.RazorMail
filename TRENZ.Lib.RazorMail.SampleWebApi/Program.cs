using TRENZ.Lib.RazorMail.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddRazorEmailRenderer();

var app = builder.Build();

app.MapControllers();

app.Run();