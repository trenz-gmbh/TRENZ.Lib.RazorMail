![MIT License](https://img.shields.io/badge/License-MIT-green?style=flat-square)
![Core NuGet Version](https://img.shields.io/nuget/v/TRENZ.Lib.RazorMail.Core?style=flat-square&label=Core)
![MailKit NuGet Version](https://img.shields.io/nuget/v/TRENZ.Lib.RazorMail.MailKit?style=flat-square&label=MailKit)
![SystemNet NuGet Version](https://img.shields.io/nuget/v/TRENZ.Lib.RazorMail.SystemNet?style=flat-square&label=SystemNet)
![NuGet Downloads](https://img.shields.io/nuget/dt/TRENZ.Lib.RazorMail.Core?style=flat-square)

# TRENZ.Lib.RazorMail

## Templated transactional e-mail using Razor

This is a simple library you can use to write e-mail templates
in [Razor syntax](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor). That means you write raw HTML, but
elevated with C# — you get `@foreach`, `@switch`, and so on, _and_ you get a strongly-typed model for custom data.

## Installation

> [!NOTE]
> You currently need to create an ASP.NET app to use the razor mail renderer.
> See [#7](https://github.com/trenz-gmbh/TRENZ.Lib.RazorMail/issues/7) for more information.

In NuGet, reference one of the available packages, depending on which `MailSender` backend you prefer. Currently, there
are three available:

* [MailKit](https://github.com/jstedfast/MailKit) is more modern and powerful and can be referenced via
  `TRENZ.Lib.RazorMail.MailKit`
* `System.Net.Mail` comes built into .NET and can be referenced via `TRENZ.Lib.RazorMail.SystemNet`
* Using the [Mircosoft Graph API](https://learn.microsoft.com/en-us/graph/overview) it is possible to send Mails via
  Outlook without using SMTP. It can be referenced via `TRENZ.Lib.RazorMail.MicrosoftGraph`.

There is no need to reference `TRENZ.Lib.RazorMail` directly.

Via dotnet:

Using `System.Net.Mail`

```bash
dotnet add package TRENZ.Lib.RazorMail.SystemNet
```

Using MailKit

```bash
dotnet add package TRENZ.Lib.RazorMail.MailKit
```

Using Microsoft Graph

```bash
dotnet add package TRENZ.Lib.RazorMail.MicrosoftGraph
```

## Usage

In your `Program.cs`:

```csharp
// use the default WebApplication builder (required for Razor engine)
var builder = WebApplication.CreateBuilder(args);

builder.AddControllers(); // NOT AddControllersWithViews()

builder.Services.AddRazorMailRenderer();

builder.Services.AddMailKitMailClient();
// or
builder.Services.AddSystemNetMailClient();
// or
builder.Services.AddMsGraphMailClient();

var app = builder.Build();

app.MapControllers();

// ...
```

A simple template looks like so:

```cshtml
@using TRENZ.Lib.RazorMail.SampleWebApi.Models

@inherits TRENZ.Lib.RazorMail.Core.MailTemplateBase<SampleModel>

@{
    Subject = "Greetings!";
}

<!DOCTYPE html>

<html lang="en">
<head>
    <title></title>
</head>
<body>
    <h1>@Model.Salutation!</h1>
</body>
</html>
```

Where `SampleModel` is:

```csharp
public record SampleModel(string Salutation);
```

Notice that:

* we're passing `SampleModel` as our model type. It has a property `Salutation`, so we can then do `@Model.Salutation`
  to get its value.
* we can set the `Subject` property, which becomes the e-mail subject.

## Attachments

Inheriting from `TRENZ.Lib.RazorMail.Core.MailTemplateBase<T>` also gives us the convenience methods `AttachFile()` and
`InlineFile()`. These differ only in whether an attachment is intended for download, or for inline display.

For example, to show an image inline, you simply do:

```cshtml
<img src="@InlineFile("My Company Logo.png")" />
```

That's it. This attaches the image as a file, then references it using `cid` format[^1]. Because the image is attached,
this also doesn't require your users to enable loading external images, which some mail clients restricts for privacy
reasons.

[^1]: Each attachment becomes part of a [MIME multipart message](https://en.wikipedia.org/wiki/MIME#Multipart_messages),
and is identified by its Content-ID. To _refer_ to that part, RazorMail then uses the `cid:(Content-ID)` URI scheme.

Or, to attach a file:

```cshtml
@{
    AttachFile("Invoice.pdf", someByteArray);
}
```

(Because file attachments don't relate to the body, you probably want to put this near the `Subject`.)

## Sending

Depending on which NuGet package you've picked above, you get a backend for sending either via the classic
`System.Net.Mail`, or via `MailKit`/MimeKit.

You can also pass a callback to the mail clients to configure default headers (setting a global `From` header, for
example).

> [!NOTE]
> The `IMailRenderer` is a scoped service.
> This means you will either need to register your service as a scoped service or obtain a scope using this code:
>
> ```csharp
> await using var scope = serviceProvider.CreateScopeAsync();
> var renderer = scope.ServiceProvider.GetRequiredService<IMailRenderer>();
> ```

This is how you would actually render and send an e-mail:

```csharp
const string viewName = "Sample";

// this is your model. If you're sending to multiple people, you may want to customize this per person.
var model = new SampleModel(request.Salutation);

// this renders your view (your e-mail template), with the above model as an argument).
IMailRenderer renderer = ...; // inject via DI, for example
var content = await renderer.RenderAsync(viewName, model);

// this wraps the rendered HTML and passes it to a service that handles sending.
// You need someone to send from, and one or more recipients.
var mail = new MailMessage
{
    Headers = new()
    {
        Recipients = [request.To],
        // here you can add CC, BCC and other headers
    },
    Content = content,
};

// this actually sends the e-mail
IMailClient client = ...; // inject via DI or use MailKitMailClient or SystemNetMailClient directly
await client.SendAsync(mail);
```

## Microsoft Graph Considerations

The Microsoft Graph API (MsGraph) is a powerful API provided by Microsoft. There are currently two modes in which this
Liberary can be used with MsGraph. It is able to send mails either on behalf of user (delegated) or as every user in a
tenant (application). Configuration of the MsGraph portion of this library is done via the `appsettings.json` file.
Example `.json` file:

```json
{
  "MsGraphOptions": {
    "TenantId": "your_tenant_id",
    "ClientId": "your_client_id",
    "ClientSecret": "your_secret",
    "SaveToSentItems": false,
    "RedirectUri": "your_redirect_id",
    "Delegated": true
  }
}
```

In order for the MsGraph portion of this library to be able to work properly it needs to have proper permissions set for
your chosen mode. MsGraph uses
the [Microsoft identity platform](https://learn.microsoft.com/en-us/entra/identity-platform/v2-overview) to handle
authentication. To be able to use this platform an app registration via
the [Microsoft Entra Admin center](https://entra.microsoft.com/) is needed. A reference and starting point can be
found [here](https://learn.microsoft.com/en-us/graph/auth/auth-concepts).
`TenantId` and `ClientId` can be found in that app registration, the `ClientSecret` can be created there as well. Note
that `SaveToSentItems` which is intended to grant the option of saving a sent mail to the inbox only works if a mail is
sent without attachments.

### With Application Permissions

If it is wished to run this library with application level permissions, the field `RedirectUri` is not needed and
`Delegated` should be set to `false`. The needed Permission in that case are `Mail.ReadWrite` and `Mail.Send` (type
Application). Note that with application level permissions mails can be sent as any user in the tenant. The Permission
`Mail.ReadWrite` is additionally needed because of the way attachments are done in Outlook, see the Microsoft
documentation [here](https://learn.microsoft.com/en-us/graph/outlook-large-attachments?tabs=http) for more details.

### With Delegated Permission

Using this library on behalf of a user necessitates authentication of that user. The authentication is done on the basis
of
the [OAuth 2.0 authorization code grant flow](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow)
as described by Microsoft. The persmissions needed are `Mail.ReadWrite`, `Mail.Send`, `User.Read` and `offline_access`
(type Delegated). In order for the authentication process to work a browser needs to be installed. Furthermore a
redirect uri must be defined and set in the `appsettings.json` as well as in the app registration. This redirect uri
should point towards your application where the authorization code can be handed to the
`InitializeGraphClientViaAuthCode()` function defined in `MsGraphDelegatedMailClient`. This is the `IMailClient` which
is registered via `AddMsGraphMailClient()` when `Delegated` is `true` in the `appsettings.json`.
