using GestordeTaras.EN;
using Microsoft.AspNetCore.Mvc;

namespace GestordeTareas.UI.Services
{
    public interface ILoginService
    {
        Task<IActionResult> HandleLoginAsync(Usuario user, string returnUrl, Controller controller);
    }
}
