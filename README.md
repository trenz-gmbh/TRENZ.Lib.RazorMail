# TRENZ.Lib.RazorMail
## Templated transactional e-mail using Razor

This is a simple library you can use to write e-mail templates in [Razor syntax](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor). That means you write raw HTML, but elevated with C# — you get `@foreach`, `@switch`, and so on, _and_ you get a strongly-typed model for custom data.

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

* we're passing `SampleModel` as our model type. It has a property `Saluation`, so we can then do `@Model.Salutation` to get its value.
* we can set the `Subject` property, which becomes the e-mail subject.

## Attachments

Inheriting from `TRENZ.Lib.RazorMail.Core.MailTemplateBase<T>` also gives us the convenience methods `AttachFile()` and `InlineFile()`. These differ only in whether an attachment is intended for download, or for inline display.

For example, to show an image inline, you simply do:

```cshtml
<img src="@InlineFile("My Company Logo.png")" />
```

That's it. This attaches the image as a file, then references it using `cid` format. Because the image is attached, this also doesn't require your users to enable loading external images.

Or, to attach a file:

```cshtml
@{
    AttachFile("Invoice.pdf", someByteArray);
}
```

(Because file attachments don't relate to the body, you probably want to put this near the `Subject`.)

## Sending

The library currently contains providers for sending via the classic System.Net.Mail, or via SmtpKit.

To wrap it all up:

```csharp
const string view = "Sample";
// this is your model. If you're sending to multiple people, you may want to customize this per person.
var model = new SampleModel(request.Salutation);
// this renders your view (your e-mail template), with the above model as an argument).
var renderedMail = await EmailRenderer.RenderAsync(view, model);

// this wraps the rendered HTML and passes it to a service that handles sending. You need someone to send from, and one or more recipients.
var mail = new MailSender(from: request.From,
                          to: new[] { (MailAddress)request.To }.ToList(),
                          renderedMail);

// this calls the actual provider for sending e-mail
await mail.SendViaSystemNetMailAsync(SmtpAccount);
