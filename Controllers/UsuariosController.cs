using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    [Authorize(Roles = "administrador")]
    public class UsuariosController : Controller
    {
        private readonly IRepositorioUsuario _repositorio;

        public UsuariosController(IRepositorioUsuario repositorio)
        {
            _repositorio = repositorio;
        }

        public IActionResult Index()
        {
            var lista = _repositorio.ObtenerTodos();
            return View(lista);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Convertir ViewModel a modelo de dominio, HASHEANDO la contraseña
                var usuario = new Usuario
                {
                    Email = model.Email,
                    PasswordHash = PasswordHelper.GenerarHash(model.Password),
                    NombreCompleto = model.NombreCompleto,
                    Rol = model.Rol,
                    Avatar = model.Avatar
                };

                _repositorio.Alta(usuario);
                TempData["Mensaje"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewBag.Error = "No se pudo crear el usuario (posible email duplicado).";
                return View(model);
            }
        }

        public IActionResult Edit(int id)
        {
            var usuario = _repositorio.ObtenerPorId(id);
            if (usuario == null) return NotFound();

            // Mapear a ViewModel para el formulario
            var model = new UsuarioViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Email = usuario.Email,
                Password = "", // No mostramos el hash
                NombreCompleto = usuario.NombreCompleto,
                Rol = usuario.Rol,
                Avatar = usuario.Avatar
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, UsuarioViewModel model)
        {
            if (id != model.IdUsuario) return BadRequest();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var existente = _repositorio.ObtenerPorId(id);
                if (existente == null) return NotFound();

                // Si dejó la contraseña vacía, conservamos la anterior
                var nuevoHash = string.IsNullOrWhiteSpace(model.Password)
                    ? existente.PasswordHash
                    : PasswordHelper.GenerarHash(model.Password);

                existente.Email = model.Email;
                existente.PasswordHash = nuevoHash;
                existente.NombreCompleto = model.NombreCompleto;
                existente.Rol = model.Rol;
                existente.Avatar = model.Avatar;

                _repositorio.Modificacion(existente);
                TempData["Mensaje"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewBag.Error = "No se pudo actualizar el usuario.";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                _repositorio.Baja(id);
                TempData["Mensaje"] = "Usuario eliminado correctamente.";
            }
            catch
            {
                TempData["Error"] = "No se puede eliminar: el usuario está asociado a registros.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}