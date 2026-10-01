using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    // Solo el administrador puede gestionar usuarios.
    // El rol viene del claim ClaimTypes.Role que se arma en AccountController.
    [Authorize(Roles = "administrador")]
    public class UsuariosController : Controller
    {
        private static readonly string[] RolesValidos = { "administrador", "empleado" };

        private readonly IRepositorioUsuario _repo;

        public UsuariosController(IRepositorioUsuario repo)
        {
            _repo = repo;
        }

        // GET: Usuarios
        public IActionResult Index()
        {
            return View(_repo.ObtenerTodos());
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View(new UsuarioViewModel());
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UsuarioViewModel model)
        {
            // En el alta la contraseña es obligatoria
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "La contraseña es obligatoria");
            }

            if (!RolesValidos.Contains(model.Rol))
            {
                ModelState.AddModelError(nameof(model.Rol), "Rol inválido");
            }

            if (_repo.ObtenerPorEmail(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Ya existe un usuario con ese email");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _repo.Alta(new Usuario
                {
                    Email = model.Email.Trim(),
                    NombreCompleto = model.NombreCompleto.Trim(),
                    Rol = model.Rol,
                    PasswordHash = PasswordHelper.GenerarHash(model.Password!)
                });

                TempData["Mensaje"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ViewBag.Error = "No se pudo crear el usuario. Intentá de nuevo.";
                return View(model);
            }
        }

        // GET: Usuarios/Edit/5
        public IActionResult Edit(int id)
        {
            var u = _repo.ObtenerPorId(id);
            if (u == null) return NotFound();

            // Nunca se manda el hash a la vista
            return View(new UsuarioViewModel
            {
                IdUsuario = u.IdUsuario,
                NombreCompleto = u.NombreCompleto,
                Email = u.Email,
                Rol = u.Rol
            });
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, UsuarioViewModel model)
        {
            if (id != model.IdUsuario) return BadRequest();

            var existente = _repo.ObtenerPorId(id);
            if (existente == null) return NotFound();

            if (!RolesValidos.Contains(model.Rol))
            {
                ModelState.AddModelError(nameof(model.Rol), "Rol inválido");
            }

            // Email repetido en OTRO usuario
            var otro = _repo.ObtenerPorEmail(model.Email);
            if (otro != null && otro.IdUsuario != id)
            {
                ModelState.AddModelError(nameof(model.Email), "Ya existe un usuario con ese email");
            }

            // No dejar al sistema sin ningún administrador
            // (por ejemplo, si el admin se cambia su propio rol a empleado)
            if (existente.Rol == "administrador" && model.Rol != "administrador" && CantidadAdministradores() <= 1)
            {
                ModelState.AddModelError(nameof(model.Rol), "No se puede quitar el rol al único administrador");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                existente.Email = model.Email.Trim();
                existente.NombreCompleto = model.NombreCompleto.Trim();
                existente.Rol = model.Rol;

                // Contraseña vacía = conservar la actual
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    existente.PasswordHash = PasswordHelper.GenerarHash(model.Password);
                }

                _repo.Modificacion(existente);
                TempData["Mensaje"] = "Usuario modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ViewBag.Error = "No se pudo modificar el usuario. Intentá de nuevo.";
                return View(model);
            }
        }

        // POST: Usuarios/Delete/5
        // (la confirmación se hace con confirm() en el Index, por eso no hay vista Delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var u = _repo.ObtenerPorId(id);
            if (u == null) return NotFound();

            if (id == ObtenerIdUsuarioActual())
            {
                TempData["Error"] = "No podés eliminar tu propio usuario.";
                return RedirectToAction(nameof(Index));
            }

            if (u.Rol == "administrador" && CantidadAdministradores() <= 1)
            {
                TempData["Error"] = "No se puede eliminar al único administrador.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _repo.Baja(id);
                TempData["Mensaje"] = "Usuario eliminado correctamente.";
            }
            catch (Exception)
            {
                // Lo más probable: el usuario figura en reservas o pagos (clave foránea)
                TempData["Error"] = "No se pudo eliminar: el usuario tiene reservas o pagos asociados.";
            }

            return RedirectToAction(nameof(Index));
        }

        private int CantidadAdministradores()
        {
            return _repo.ObtenerTodos().Count(x => x.Rol == "administrador");
        }

        private int ObtenerIdUsuarioActual()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : 0;
        }
    }
}