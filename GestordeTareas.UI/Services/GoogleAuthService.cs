using GestordeTaras.EN;
using GestordeTareas.BL;
using GestordeTareas.UI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly UsuarioBL _usuarioBL;
        private readonly CargoBL _cargoBL;

        public GoogleAuthService(UsuarioBL usuarioBL, CargoBL cargoBL)
        {
            _usuarioBL = usuarioBL;
            _cargoBL = cargoBL;
        }

        public async Task<IActionResult> HandleGoogleLoginAsync(HttpContext httpContext, string returnUrl)
        {
            // 1️⃣ Autenticación externa
            var authenticateResult = await httpContext.AuthenticateAsync("External");

            if (!authenticateResult.Succeeded)
                return new RedirectToActionResult("Login", "Cuenta", null);

            var claims = authenticateResult.Principal.Claims;

            // 2️⃣ Email
            string email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (email == null)
            {
                httpContext.Items["ErrorMessage"] = "Google no proporcionó un correo electrónico.";
                return new RedirectToActionResult("Login", "Cuenta", null);
            }

            // 3️⃣ AccessToken
            string accessToken = authenticateResult.Properties.GetTokenValue("access_token");

            string nombre = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "Usuario";
            string apellido = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
            string foto = null;

            // 4️⃣ API Google → obtener foto real
            if (!string.IsNullOrEmpty(accessToken))
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    nombre = root.GetProperty("given_name").GetString() ?? nombre;
                    apellido = root.TryGetProperty("family_name", out var fam) ? fam.GetString() : apellido;
                    foto = root.TryGetProperty("picture", out var pic) ? pic.GetString() : null;
                }
            }

            // 5️⃣ Buscar usuario
            var existingUser = await _usuarioBL.GetByNombreUsuarioAsync(new Usuario { NombreUsuario = email });
            Usuario userDb;

            if (existingUser == null)
            {
                userDb = new Usuario
                {
                    Nombre = nombre,
                    Apellido = apellido,
                    NombreUsuario = email,
                    Pass = null,
                    Telefono = null,
                    FechaNacimiento = null,
                    Status = (byte)User_Status.ACTIVO,
                    FechaRegistro = DateTime.Now,
                    IdCargo = await _cargoBL.GetCargoColaboradorIdAsync(),
                    FotoPerfil = foto ?? "/img/npc.png"
                };

                await _usuarioBL.Create(userDb);
            }
            else
            {
                userDb = existingUser;

                userDb.Nombre = nombre;
                userDb.Apellido = apellido;
                userDb.FotoPerfil = foto ?? userDb.FotoPerfil;

                await _usuarioBL.Update(userDb);
            }

            // 6️⃣ Cargar cargo
            userDb.Cargo = await _cargoBL.GetById(new Cargo { Id = userDb.IdCargo });

            // 7️⃣ Claims
            var cookieClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userDb.NombreUsuario),
                new Claim(ClaimTypes.Role, userDb.Cargo.Nombre),
                new Claim(ClaimTypes.GivenName, userDb.Nombre),
                new Claim(ClaimTypes.Surname, userDb.Apellido ?? ""),
                new Claim(ClaimTypes.NameIdentifier, userDb.Id.ToString()),
                new Claim("FotoPerfil", userDb.FotoPerfil ?? "/img/npc.png")
            };

            var claimsIdentity = new ClaimsIdentity(cookieClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // 8️⃣ Logout externo
            await httpContext.SignOutAsync("External");

            return new RedirectResult(returnUrl);
        }
    }
