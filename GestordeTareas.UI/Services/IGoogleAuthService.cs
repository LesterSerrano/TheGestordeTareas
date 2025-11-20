using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GestordeTareas.UI.Services
{
    public interface IGoogleAuthService
    {
        Task<IActionResult> HandleGoogleLoginAsync(HttpContext httpContext, string returnUrl);
    }

}
