using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GestordeTareas.BL;
using GestordeTaras.EN;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestordeTareas.UI.Helpers;
using GestordeTareas.DAL;
//using AutoMapper;
using Microsoft.AspNetCore.Authentication.Google;
using GestordeTareas.BL.Services;
using GestordeTareas.UI.Services;


namespace GestordeTareas.UI.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class UsuarioController : Controller
    {
        private readonly UsuarioBL _usuarioBL;
        private readonly CargoBL _cargoBL;
        private readonly IEmailService _emailService;
        private readonly ISeguridadService _seguridadService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly ILoginService _loginService;
        private readonly IUserCreateService _userCreateService;
        //private readonly IMapper _mapper;

        public UsuarioController(UsuarioBL usuarioBL, CargoBL cargoBL, IEmailService emailService, IGoogleAuthService googleAuthService, ISeguridadService seguridadService, ILoginService loginService, IUserCreateService userCreateService)
        {
            _usuarioBL = usuarioBL;
            _cargoBL = cargoBL;
            _emailService = emailService;
            _googleAuthService = googleAuthService;
            _seguridadService = seguridadService;
            _loginService = loginService;
            _userCreateService = userCreateService;
            //_mapper = mapper;
        }

        #region Helpers
        private async Task LoadDropDownListsAsync()
        {
            var cargos = await _cargoBL.GetAllAsync();
            ViewBag.Cargos = new SelectList(cargos, "Id", "Nombre");
        }

        private async Task ActualizarClaimsUsuario(Usuario usuario)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;

            claimsIdentity.RemoveClaim(claimsIdentity.FindFirst(ClaimTypes.GivenName));
            claimsIdentity.RemoveClaim(claimsIdentity.FindFirst(ClaimTypes.Surname));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.GivenName, usuario.Nombre));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Surname, usuario.Apellido));

            claimsIdentity.RemoveClaim(claimsIdentity.FindFirst("FotoPerfil"));
            claimsIdentity.AddClaim(new Claim("FotoPerfil", usuario.FotoPerfil ?? "/img/npc.png"));

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }

        private bool ValidarContrasena(string nueva, string confirmar, out string mensaje)
        {
            if (string.IsNullOrWhiteSpace(nueva) || string.IsNullOrWhiteSpace(confirmar))
            {
                mensaje = "Por favor, ingresa ambas contraseñas"; return false;
            }
            if (nueva != confirmar)
            {
                mensaje = "Las contraseñas no coinciden"; return false;
            }
            if (nueva.Length < 8)
            {
                mensaje = "La contraseña debe tener al menos 8 caracteres"; return false;
            }
            mensaje = null;
            return true;
        }

        #endregion

        #region Vistas Principales

        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Index(string query = "", string filter = "NombreUsuario", int top = 10)
        {
            if (top <= 0) top = 10;
            var user = new Usuario { Top_Aux = top };

            var lista = await _usuarioBL.SearchIncludeRoleAsync(user, query, filter);
            lista = lista.OrderBy(u => u.Id).ToList();

            ViewBag.Top = top;
            ViewBag.Roles = await _cargoBL.GetAllAsync();

            return View("Index", lista);
        }

  
        [Authorize]
        public async Task<ActionResult> Perfil()
        {
            try
            {
                await LoadDropDownListsAsync();

                var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

                if (idClaim == null || !int.TryParse(idClaim.Value, out int userId))
                {
                    TempData["ErrorMessage"] = "Error al identificar el ID de usuario de la sesión.";
                    return RedirectToAction("Login");
                }

                // 2. Usar el ID para obtener el usuario directamente (la forma correcta)
                var actualUser = await _usuarioBL.GetByIdAsync(new Usuario { Id = userId });

                if (actualUser == null) return NotFound();

                ViewBag.NombreUsuario = $"{actualUser.Nombre} {actualUser.Apellido}";

                return View(actualUser);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al cargar la información del usuario: " + ex.Message;
                return View();
            }
        }

        
        [AllowAnonymous]
        public IActionResult LoginGoogle(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "Usuario", new { returnUrl })
            };
            return Challenge(properties, "Google");
        }

        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
        {
            return await _googleAuthService.HandleGoogleLoginAsync(HttpContext, returnUrl);
        }


        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ViewBag.Url = returnUrl;
            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Usuario user, string returnUrl = null)
        {
            return await _loginService.HandleLoginAsync(user, returnUrl, this);
        }




        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Usuario");
        }

        #endregion

        #region CRUD Usuarios

        [AllowAnonymous]
        public async Task<IActionResult> Create()
        {
            await LoadDropDownListsAsync();
            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Create(Usuario usuario, IFormFile fotoPerfil)
        {
            return _userCreateService.HandleCreateAsync(usuario, fotoPerfil, this);
        }



        [Authorize]
        public async Task<ActionResult> Details(int id)
        {
            var usuario = await _usuarioBL.GetByIdAsync(new Usuario { Id = id });
            if (usuario == null) return NotFound();

            return PartialView("Details", usuario);
        }

        public async Task<ActionResult> Edit(int id)
        {
            var usuario = await _usuarioBL.GetByIdAsync(new Usuario { Id = id });
            if (usuario == null) return NotFound();

            await LoadDropDownListsAsync();
            return PartialView("Edit", usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Edit(int id, Usuario usuario)
        {
            try
            {
                var existingUser = await _usuarioBL.GetByIdAsync(new Usuario { Id = id });
                if (existingUser == null) return Json(new { success = false, message = "Usuario no encontrado" });

                usuario.Pass = existingUser.Pass;
                await _usuarioBL.Update(usuario);

                return Json(new { success = true, message = "Usuario actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditOwn(Usuario usuario, string currentPassword, IFormFile fotoPerfil)
        {
            try
            {
                var existingUser = await _usuarioBL.GetByIdAsync(usuario);
                if (existingUser == null) return RedirectToAction("Perfil");

                string nuevaPasswordHasheada = null;
                if (!string.IsNullOrEmpty(usuario.Pass) && usuario.Pass.Length >= 8)
                {
                    if (!_seguridadService.VerifyPassword(currentPassword, existingUser.Pass))
                    {
                        TempData["ErrorMessage"] = "Contraseña actual incorrecta";
                        return RedirectToAction("Perfil");
                    }
                    if (usuario.Pass != usuario.ConfirmarPass)
                    {
                        TempData["ErrorMessage"] = "La nueva contraseña y la confirmación no coinciden";
                        return RedirectToAction("Perfil");
                    }
                    nuevaPasswordHasheada = _seguridadService.HashPassword(usuario.Pass);
                }

                if (fotoPerfil != null && fotoPerfil.Length > 0)
                {
                    existingUser.FotoPerfil = await ImageHelper.SubirArchivo(fotoPerfil.OpenReadStream(), fotoPerfil.FileName);
                }

                // Crear objeto limpio
                var usuarioActualizado = new Usuario
                {
                    Id = existingUser.Id,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Telefono = usuario.Telefono,
                    FechaNacimiento = usuario.FechaNacimiento,
                    NombreUsuario = usuario.NombreUsuario,
                    Status = existingUser.Status,
                    FotoPerfil = existingUser.FotoPerfil,
                    IdCargo = existingUser.IdCargo,
                    Pass = nuevaPasswordHasheada // null si no se cambió, hash si se cambió
                };

                await _usuarioBL.Update(usuarioActualizado);
                await ActualizarClaimsUsuario(existingUser);
                TempData["SuccessMessage"] = "Perfil actualizado correctamente";
                return RedirectToAction("Perfil");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Perfil");
            }
        }


        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _usuarioBL.GetByIdAsync(new Usuario { Id = id });

            if (usuario == null)
            {
                return NotFound();
            }

            return PartialView("Delete", usuario);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, Usuario usuario)
        {
            try
            {
                await _usuarioBL.Delete(usuario);
                return Json(new { success = true, message = "Usuario eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteOwn()
        {
            try
            {
                var nombreUsuario = User.Identity.Name;
                var usuarioDB = await _usuarioBL.GetByNombreUsuarioAsync(new Usuario { NombreUsuario = nombreUsuario });

                if (usuarioDB == null) return RedirectToAction("Perfil");

                await _usuarioBL.Delete(usuarioDB);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                TempData["SuccessMessage"] = "Cuenta eliminada correctamente";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Perfil");
            }
        }

        #endregion
    }
}
