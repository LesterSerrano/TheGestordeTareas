using GestordeTaras.EN;
using GestordeTareas.UI.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestordeTareas.UI.Services
{
    public class UserCreateService : IUserCreateService
    {
        private readonly BL.UsuarioBL _usuarioBL;
        private readonly BL.CargoBL _cargoBL;

        public UserCreateService(BL.UsuarioBL usuarioBL, BL.CargoBL cargoBL)
        {
            _usuarioBL = usuarioBL;
            _cargoBL = cargoBL;
        }

        public async Task<IActionResult> HandleCreateAsync(
            Usuario usuario,
            IFormFile? fotoPerfil,
            Controller controller)
        {
            try
            {
                // --- VALIDAR CONTRASEÑA ---
                if (string.IsNullOrEmpty(usuario.Pass) || usuario.Pass.Length < 8)
                {
                    controller.TempData["ErrorMessage"] = "La contraseña debe tener al menos 8 caracteres";
                    await CargarDropDowns(controller);
                    return controller.View(usuario);
                }

                usuario.Status = (byte)User_Status.ACTIVO;

                // --- SUBIR FOTO ---
                if (fotoPerfil != null && fotoPerfil.Length > 0)
                {
                    usuario.FotoPerfil = await ImageHelper.SubirArchivo(
                        fotoPerfil.OpenReadStream(),
                        fotoPerfil.FileName
                    );
                }
                else
                {
                    usuario.FotoPerfil = "/img/npc.png";
                }

                // --- SI ES ADMIN ---
                if (controller.User.IsInRole("Administrador"))
                {
                    await _usuarioBL.Create(usuario);
                    controller.TempData["SuccessMessage"] = "Usuario creado correctamente";
                    return controller.RedirectToAction("Index");
                }

                // --- SI NO ES ADMIN: ASIGNAR CARGO COLABORADOR ---
                var cargoColaboradorId = await _cargoBL.GetCargoColaboradorIdAsync();
                usuario.IdCargo = cargoColaboradorId;

                // Crear usuario
                await _usuarioBL.Create(usuario);

                // Cargar cargo para claims
                usuario.Cargo = await _cargoBL.GetById(new Cargo { Id = usuario.IdCargo });

                // --- INICIAR SESIÓN ---
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.Role, usuario.Cargo.Nombre),
                    new Claim(ClaimTypes.GivenName, usuario.Nombre),
                    new Claim(ClaimTypes.Surname, usuario.Apellido ?? ""),
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim("FotoPerfil", usuario.FotoPerfil ?? "/img/npc.png")
                };

                await controller.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme))
                );

                controller.TempData["SuccessMessage"] = "Has iniciado sesión correctamente.";
                return controller.RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                controller.TempData["ErrorMessage"] = ex.Message;
                await CargarDropDowns(controller);
                return controller.View(usuario);
            }
        }

        private async Task CargarDropDowns(Controller controller)
        {
            var cargos = await _cargoBL.GetAllAsync();
            controller.ViewBag.Cargos = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cargos, "Id", "Nombre");
        }
    }
}
