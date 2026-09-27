using System.Linq;
using Microsoft.AspNetCore.Authorization; // Agregado para [Authorize]
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    [Authorize] // Agregado: Protege todo el controlador
    public class InquilinosController : Controller
    {
        private readonly IRepositorioInquilino repositorio;

        public InquilinosController(IRepositorioInquilino repositorio)
        {
            this.repositorio = repositorio;
        }

        // GET: Inquilinos (CON BÚSQUEDA)
        public IActionResult Index(string? busqueda)
        {
            var lista = repositorio.ObtenerTodos();

            // Si el usuario escribió algo, filtramos por Nombre o DNI
            if (!string.IsNullOrEmpty(busqueda))
            {
                lista = lista.Where(i => 
                    i.NombreCompleto.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    i.Dni.Contains(busqueda, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Guardamos el término para que el input no se borre al recargar
            ViewBag.BusquedaActual = busqueda;
            
            return View(lista);
        }

        // GET: Inquilinos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inquilinos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Inquilino i)
        {
            if (!ModelState.IsValid)
            {
                return View(i);
            }

            try
            {
                repositorio.Alta(i);
                TempData["Mensaje"] = "Inquilino creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo crear el inquilino: " + ex.Message;
                return View(i);
            }
        }

        // GET: Inquilinos/Details/5
        public IActionResult Details(int id)
        {
            var inquilino = repositorio.ObtenerPorId(id);
            if (inquilino == null)
            {
                return NotFound();
            }
            return View(inquilino);
        }

        // GET: Inquilinos/Edit/5
        public IActionResult Edit(int id)
        {
            var i = repositorio.ObtenerPorId(id);
            if (i == null)
            {
                return NotFound();
            }
            return View(i);
        }

        // POST: Inquilinos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Inquilino i)
        {
            if (id != i.IdInquilino)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(i);
            }

            try
            {
                repositorio.Modificacion(i);
                TempData["Mensaje"] = "Inquilino modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo modificar el inquilino: " + ex.Message;
                return View(i);
            }
        }

        // GET: Inquilinos/Delete/5
        public IActionResult Delete(int id)
        {
            var i = repositorio.ObtenerPorId(id);
            if (i == null)
            {
                return NotFound();
            }
            return View(i);
        }

        // POST: Inquilinos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                repositorio.Baja(id);
                TempData["Mensaje"] = "Inquilino eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo eliminar el inquilino: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}