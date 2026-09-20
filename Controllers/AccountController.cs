using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepositorioUsuario _repoUsuario;

        public AccountController(IRepositorioUsuario repoUsuario)
        {
            _repoUsuario = repoUsuario;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // 1. Buscar el usuario en la base de datos
                var usuario = _repoUsuario.ObtenerPorEmail(model.Email);

                // 2. Validar que exista y la contraseña coincida 
                // (Nota: En producción real, aquí se usaría BCrypt o similar para comparar hashes)
                if (usuario != null && usuario.PasswordHash == model.Password)
                {
                    // 3. Crear las Claims (la identidad del usuario)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                        new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                        new Claim(ClaimTypes.Role, usuario.Rol) // Ej: "administrador" o "empleado"
                    };

                    // 4. Crear la identidad y el principal
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // 5. Iniciar sesión (crea la cookie)
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    // 6. Redirigir a la página solicitada o al Home
                    return LocalRedirect(returnUrl ?? "/");
                }

                // Si falla, mostrar error genérico por seguridad
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}