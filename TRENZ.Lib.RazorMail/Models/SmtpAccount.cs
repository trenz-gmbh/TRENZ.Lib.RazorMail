namespace TRENZ.Lib.RazorMail.Models;

public record SmtpAccount(string Host, int Port,
                          bool TLS,
                          string Login, string Password);
