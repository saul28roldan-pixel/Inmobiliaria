using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    public class PropietariosController : Controller
    {
        private readonly IRepositorioPropietario repositorio;

        // El framework inyecta automáticamente la implementación
        // registrada en Program.cs (AddScoped<IRepositorioPropietario, RepositorioPropietario>)
        public PropietariosController(IRepositorioPropietario repositorio)
        {
            this.repositorio = repositorio;
        }

        // GET: Propietarios
        public IActionResult Index()
        {
            var lista = repositorio.ObtenerTodos();
            return View(lista);
        }

        // GET: Propietarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Propietarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Propietario p)
        {
            if (!ModelState.IsValid)
            {
                return View(p);
            }

            try
            {
                repositorio.Alta(p);
                TempData["Mensaje"] = "Propietario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo crear el propietario: " + ex.Message;
                return View(p);
            }
        }

        // GET: Propietarios/Edit/5
        public IActionResult Edit(int id)
        {
            var p = repositorio.ObtenerPorId(id);
            if (p == null)
            {
                return NotFound();
            }
            return View(p);
        }

        // POST: Propietarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Propietario p)
        {
            if (id != p.IdPropietario)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(p);
            }

            try
            {
                repositorio.Modificacion(p);
                TempData["Mensaje"] = "Propietario modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo modificar el propietario: " + ex.Message;
                return View(p);
            }
        }

        // GET: Propietarios/Delete/5
        public IActionResult Delete(int id)
        {
            var p = repositorio.ObtenerPorId(id);
            if (p == null)
            {
                return NotFound();
            }
            return View(p);
        }
        // GET: Propietarios/Details/5
      public IActionResult Details(int id)
      {
           var propietario = repositorio.ObtenerPorId(id);
           if (propietario == null)
           {
                return NotFound();
           }
           return View(propietario);
        }
        // POST: Propietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                repositorio.Baja(id);
                TempData["Mensaje"] = "Propietario eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo eliminar el propietario: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}