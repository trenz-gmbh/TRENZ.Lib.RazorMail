namespace TRENZ.Lib.RazorMail.SampleWebApi.Models;

public record SendSampleMailRequestWithOptions(string From, string[] To, string Salutation, int? PerFileSizeMb, int? FileAmount, string? Importance) : SendSampleMailRequest(From, To, Salutation);
