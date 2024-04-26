namespace TRENZ.Lib.RazorMail.SampleWebApi.Models;

public record SendSampleMailRequest(string From, string[] To, string Salutation);
