using System.Linq;
using Microsoft.AspNetCore.Authorization; // Agregado para [Authorize]
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    [Authorize] // Agregado: Protege todo el controlador
    public class TipoInmuebleController : Controller
    {
        private readonly IRepositorioTipoInmueble repositorio;

        public TipoInmuebleController(IRepositorioTipoInmueble repositorio)
        {
            this.repositorio = repositorio;
        }

        // GET: TipoInmueble (CON BÚSQUEDA)
        public IActionResult Index(string? busqueda)
        {
            var lista = repositorio.ObtenerTodos();

            // Si el usuario escribió algo, filtramos por Descripción
            if (!string.IsNullOrEmpty(busqueda))
            {
                lista = lista.Where(t => 
                    t.Descripcion.Contains(busqueda, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Guardamos el término para que el input no se borre al recargar
            ViewBag.BusquedaActual = busqueda;
            
            return View(lista);
        }

        // GET: TipoInmueble/Details/5
        public IActionResult Details(int id)
        {
            var tipo = repositorio.ObtenerPorId(id);
            if (tipo == null)
            {
                return NotFound();
            }
            return View(tipo);
        }

        // GET: TipoInmueble/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoInmueble/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TipoInmueble tipo)
        {
            if (!ModelState.IsValid)
            {
                return View(tipo);
            }

            try
            {
                repositorio.Alta(tipo);
                TempData["Mensaje"] = "Tipo de inmueble creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo crear el tipo de inmueble: " + ex.Message;
                return View(tipo);
            }
        }

        // GET: TipoInmueble/Edit/5
        public IActionResult Edit(int id)
        {
            var tipo = repositorio.ObtenerPorId(id);
            if (tipo == null)
            {
                return NotFound();
            }
            return View(tipo);
        }

        // POST: TipoInmueble/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, TipoInmueble tipo)
        {
            if (id != tipo.IdTipo)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(tipo);
            }

            try
            {
                repositorio.Modificacion(tipo);
                TempData["Mensaje"] = "Tipo de inmueble modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo modificar el tipo de inmueble: " + ex.Message;
                return View(tipo);
            }
        }

        // GET: TipoInmueble/Delete/5
        public IActionResult Delete(int id)
        {
            var tipo = repositorio.ObtenerPorId(id);
            if (tipo == null)
            {
                return NotFound();
            }
            return View(tipo);
        }

        // POST: TipoInmueble/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                repositorio.Baja(id);
                TempData["Mensaje"] = "Tipo de inmueble eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo eliminar el tipo de inmueble: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}