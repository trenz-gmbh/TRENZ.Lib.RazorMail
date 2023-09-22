using TRENZ.Lib.RazorMail.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvcCore()
                .AddRazorViewEngine();

builder.Services.AddTransient<IRazorEmailRenderer, RazorEmailRenderer>();

var app = builder.Build();

app.MapControllers();

app.Run();