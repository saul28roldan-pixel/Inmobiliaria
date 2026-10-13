using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepositorioUsuario _repoUsuario;
        private readonly IWebHostEnvironment _entorno;

        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long TamanioMaximoBytes = 2 * 1024 * 1024; // 2 MB

        public AccountController(IRepositorioUsuario repoUsuario, IWebHostEnvironment entorno)
        {
            _repoUsuario = repoUsuario;
            _entorno = entorno;
        }

        // ===================== LOGIN / LOGOUT =====================

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
                var usuario = _repoUsuario.ObtenerPorEmail(model.Email);

                if (usuario != null && PasswordHelper.VerificarHash(model.Password, usuario.PasswordHash))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                        new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                        new Claim(ClaimTypes.Role, usuario.Rol)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    return LocalRedirect(returnUrl ?? "/");
                }

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

        // ===================== PERFIL PROPIO =====================

        [Authorize]
        public IActionResult Perfil()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // ----- Editar datos personales -----
        [Authorize]
        [HttpGet]
        public IActionResult EditarDatos()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null) return NotFound();

            var model = new EditarDatosViewModel
            {
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarDatos(EditarDatosViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = ObtenerUsuarioActual();
            if (usuario == null) return NotFound();

            try
            {
                usuario.NombreCompleto = model.NombreCompleto;
                usuario.Email = model.Email;
                _repoUsuario.Modificacion(usuario);

                TempData["Mensaje"] = "Datos personales actualizados correctamente.";
                return RedirectToAction(nameof(Perfil));
            }
            catch
            {
                ViewBag.Error = "No se pudieron actualizar los datos (posible email duplicado).";
                return View(model);
            }
        }

        // ----- Cambiar contraseña -----
        [Authorize]
        [HttpGet]
        public IActionResult CambiarClave()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarClave(CambiarClaveViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = ObtenerUsuarioActual();
            if (usuario == null) return NotFound();

            if (!PasswordHelper.VerificarHash(model.ClaveActual, usuario.PasswordHash))
            {
                ModelState.AddModelError("ClaveActual", "La contraseña actual es incorrecta.");
                return View(model);
            }

            try
            {
                usuario.PasswordHash = PasswordHelper.GenerarHash(model.ClaveNueva);
                _repoUsuario.Modificacion(usuario);

                TempData["Mensaje"] = "Contraseña cambiada correctamente.";
                return RedirectToAction(nameof(Perfil));
            }
            catch
            {
                ViewBag.Error = "No se pudo cambiar la contraseña.";
                return View(model);
            }
        }

        // ----- Cambiar Avatar -----
        [Authorize]
        [HttpGet]
        public IActionResult CambiarAvatar()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarAvatar(IFormFile? AvatarFile)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null) return NotFound();

            if (AvatarFile == null || AvatarFile.Length == 0)
            {
                ViewBag.Error = "Debés seleccionar una imagen.";
                return View(usuario);
            }

            try
            {
                EliminarArchivoSiExiste(usuario.Avatar);

                var ruta = GuardarAvatar(AvatarFile);
                if (ruta == null)
                {
                    ViewBag.Error = "Formato o tamaño de imagen no válido (máx 2MB, jpg/png/webp/gif).";
                    return View(usuario);
                }

                usuario.Avatar = ruta;
                _repoUsuario.Modificacion(usuario);

                TempData["Mensaje"] = "Avatar actualizado correctamente.";
                return RedirectToAction(nameof(Perfil));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al subir el avatar: " + ex.Message;
                return View(usuario);
            }
        }

        // ----- Eliminar Avatar -----
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarAvatar()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null) return NotFound();

            try
            {
                EliminarArchivoSiExiste(usuario.Avatar);
                usuario.Avatar = null;
                _repoUsuario.Modificacion(usuario);

                TempData["Mensaje"] = "Avatar eliminado correctamente.";
            }
            catch
            {
                TempData["Error"] = "No se pudo eliminar el avatar.";
            }

            return RedirectToAction(nameof(Perfil));
        }

        // ===================== HELPERS =====================

        private Usuario? ObtenerUsuarioActual()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out int id)) return null;
            return _repoUsuario.ObtenerPorId(id);
        }

        private string? GuardarAvatar(IFormFile archivo)
        {
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension) || archivo.Length > TamanioMaximoBytes)
                return null;

            var nombreArchivo = $"avatar_{Guid.NewGuid()}{extension}";
            var carpeta = Path.Combine(_entorno.WebRootPath, "img", "avatars");

            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            var rutaFisica = Path.Combine(carpeta, nombreArchivo);
            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                archivo.CopyTo(stream);
            }

            return $"/img/avatars/{nombreArchivo}";
        }

        private void EliminarArchivoSiExiste(string? rutaRelativa)
        {
            if (string.IsNullOrEmpty(rutaRelativa)) return;

            var rutaFisica = Path.Combine(_entorno.WebRootPath, rutaRelativa.TrimStart('/'));
            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }
        }
    }
}