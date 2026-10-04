using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class TipoInmuebleController : Controller
    {
        private readonly IRepositorioTipoInmueble repositorio;

        public TipoInmuebleController(IRepositorioTipoInmueble repositorio)
        {
            this.repositorio = repositorio;
        }

        // GET: TipoInmueble (CON BÚSQUEDA Y PAGINADO)
        public IActionResult Index(string? busqueda, int pagina = 1)
        {
            int registrosPorPagina = 3;

            var lista = repositorio.ObtenerFiltradosPaginados(
                busqueda, 
                pagina, 
                registrosPorPagina, 
                out int totalRegistros
            );

            ViewBag.BusquedaActual = busqueda;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

            return View(lista);
        }

        // GET: TipoInmueble/Details/5
        public IActionResult Details(int id)
        {
            var t = repositorio.ObtenerPorId(id);
            if (t == null) return NotFound();
            return View(t);
        }

        // GET: TipoInmueble/Create
        public IActionResult Create() => View();

        // POST: TipoInmueble/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TipoInmueble t)
        {
            if (!ModelState.IsValid) return View(t);

            try
            {
                repositorio.Alta(t);
                TempData["Mensaje"] = "Tipo de inmueble creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewBag.Error = "No se pudo crear el tipo de inmueble.";
                return View(t);
            }
        }

        // GET: TipoInmueble/Edit/5
        public IActionResult Edit(int id)
        {
            var t = repositorio.ObtenerPorId(id);
            if (t == null) return NotFound();
            return View(t);
        }

        // POST: TipoInmueble/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, TipoInmueble t)
        {
            if (id != t.IdTipo) return BadRequest();
            if (!ModelState.IsValid) return View(t);

            try
            {
                repositorio.Modificacion(t);
                TempData["Mensaje"] = "Tipo de inmueble modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewBag.Error = "No se pudo modificar el tipo de inmueble.";
                return View(t);
            }
        }

        // GET: TipoInmueble/Delete/5
        public IActionResult Delete(int id)
        {
            var t = repositorio.ObtenerPorId(id);
            if (t == null) return NotFound();
            return View(t);
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
            catch
            {
                TempData["Error"] = "No se puede eliminar: está asociado a inmuebles.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}