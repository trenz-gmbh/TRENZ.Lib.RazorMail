namespace TRENZ.Lib.RazorMail.SampleWebApi.Models;

public record SendSampleMailRequestWithOptions(string From, string[] To, string Salutation, int? FileSize, int? FileAmount) : SendSampleMailRequest(From, To, Salutation);
