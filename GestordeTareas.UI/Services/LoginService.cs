using GestordeTaras.EN;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestordeTareas.UI.Services
{
    public class LoginService : ILoginService
    {
        private readonly BL.UsuarioBL _usuarioBL;
        private readonly BL.CargoBL _cargoBL;

        public LoginService(BL.UsuarioBL usuarioBL, BL.CargoBL cargoBL)
        {
            _usuarioBL = usuarioBL;
            _cargoBL = cargoBL;
        }

        public async Task<IActionResult> HandleLoginAsync(
            Usuario user,
            string returnUrl,
            Controller controller)
        {
            try
            {
                // --- VALIDAR USUARIO EN BL ---
                var userDb = await _usuarioBL.LoginAsync(user);

                if (userDb == null)
                {
                    controller.TempData["ErrorMessage"] = "Usuario o contraseña incorrectos";
                    return controller.View(new Usuario { NombreUsuario = user.NombreUsuario });
                }

                // --- ESTADO INACTIVO ---
                if (userDb.Status != (byte)User_Status.ACTIVO)
                {
                    controller.TempData["ErrorMessage"] = "Tu cuenta está inactiva. Contacta al administrador.";
                    return controller.View(new Usuario { NombreUsuario = user.NombreUsuario });
                }

                // --- CARGAR CARGO (ROL) ---
                userDb.Cargo = await _cargoBL.GetById(new Cargo { Id = userDb.IdCargo });

                // --- FOTO PERFIL ---
                var fotoPerfil = string.IsNullOrEmpty(userDb.FotoPerfil)
                    ? "/img/npc.png"
                    : userDb.FotoPerfil;

                // --- CREAR CLAIMS ---
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, userDb.NombreUsuario),
                    new Claim(ClaimTypes.Role, userDb.Cargo.Nombre),
                    new Claim(ClaimTypes.GivenName, userDb.Nombre),
                    new Claim(ClaimTypes.Surname, userDb.Apellido ?? ""),
                    new Claim(ClaimTypes.NameIdentifier, userDb.Id.ToString()),
                    new Claim("FotoPerfil", fotoPerfil)
                };

                // --- INICIAR SESIÓN ---
                await controller.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(
                        new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
                ));

                // --- RETURN URL ---
                if (!string.IsNullOrWhiteSpace(returnUrl))
                    return controller.Redirect(returnUrl);

                return controller.RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                controller.TempData["ErrorMessage"] = ex.Message;
                return controller.View(new Usuario { NombreUsuario = user.NombreUsuario });
            }
        }
    }
}
