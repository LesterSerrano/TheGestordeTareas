using GestordeTaras.EN;
using Microsoft.AspNetCore.Mvc;

namespace GestordeTareas.UI.Services
{
    public interface IUserCreateService
    {
        Task<IActionResult> HandleCreateAsync(Usuario usuario, IFormFile? foto, Controller controller);
    }


}
