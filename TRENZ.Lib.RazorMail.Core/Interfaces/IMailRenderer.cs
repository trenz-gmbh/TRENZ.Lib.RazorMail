using System.Threading.Tasks;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Interfaces;

public interface IMailRenderer
{
    Task<MailContent> RenderAsync<TModel>(string viewName, TModel model);
}
