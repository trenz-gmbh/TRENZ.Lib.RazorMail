![MIT License](https://img.shields.io/badge/License-MIT-green?style=flat-square)
![Core NuGet Version](https://img.shields.io/nuget/v/TRENZ.Lib.RazorMail.Core?style=flat-square&label=Core)
![MailKit NuGet Version](https://img.shields.io/nuget/v/TRENZ.Lib.RazorMail.MailKit?style=flat-square&label=MailKit)
![SystemNet NuGet Version](https://img.shields.io/nuget/v/TRENZ.Lib.RazorMail.SystemNet?style=flat-square&label=SystemNet)
![NuGet Downloads](https://img.shields.io/nuget/dt/TRENZ.Lib.RazorMail.Core?style=flat-square)

# TRENZ.Lib.RazorMail
## Templated transactional e-mail using Razor

This is a simple library you can use to write e-mail templates in [Razor syntax](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor). That means you write raw HTML,
but elevated with C# — you get `@foreach`, `@switch`, and so on, _and_ you get a strongly-typed model for custom data.

## Installation

In NuGet, reference either the `TRENZ.Lib.RazorMail.SystemNet` or the `TRENZ.Lib.RazorMail.MailKit` package, depending
on which `MailSender` backend you prefer. [MailKit](https://github.com/jstedfast/MailKit) is more modern and powerful,
but `System.Net.Mail` comes built into .NET. There is no need to reference `TRENZ.Lib.RazorMail` directly.

Via dotnet:

```bash
dotnet add package TRENZ.Lib.RazorMail.SystemNet
```

…or:

```bash
dotnet add package TRENZ.Lib.RazorMail.MailKit
```

## Usage

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

* we're passing `SampleModel` as our model type. It has a property `Salutation`, so we can then do `@Model.Salutation` to get its value.
* we can set the `Subject` property, which becomes the e-mail subject.

## Attachments

Inheriting from `TRENZ.Lib.RazorMail.Core.MailTemplateBase<T>` also gives us the convenience methods `AttachFile()` and
`InlineFile()`. These differ only in whether an attachment is intended for download, or for inline display.

For example, to show an image inline, you simply do:

```cshtml
<img src="@InlineFile("My Company Logo.png")" />
```

That's it. This attaches the image as a file, then references it using `cid` format[^1].
Because the image is attached, this also doesn't require your users to enable
loading external images, which some mail clients restricts for privacy reasons.

[^1]: Each attachment becomes part of a [MIME multipart message](https://en.wikipedia.org/wiki/MIME#Multipart_messages),
and is identified by its Content-ID. To _refer_ to that part, RazorMail then
uses the `cid:(Content-ID)` URI scheme.

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
We recommend leveraging `Microsoft.Extensions.DependencyInjection`, by calling the following convenience methods:

```csharp
services.AddRazorMailRenderer();

services.AddMailKitMailClient();
// or
services.AddSystemNetMailClient();
```

You can also pass a callback to the mail clients to configure default headers (setting a global `From` header, for example).

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
