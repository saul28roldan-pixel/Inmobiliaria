using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    public class TipoInmuebleController : Controller
    {
        private readonly IRepositorioTipoInmueble repositorio;

        // El framework inyecta automáticamente la implementación
        // registrada en Program.cs (AddScoped<IRepositorioTipoInmueble, RepositorioTipoInmueble>)
        public TipoInmuebleController(IRepositorioTipoInmueble repositorio)
        {
            this.repositorio = repositorio;
        }

        // GET: TipoInmueble
        public IActionResult Index()
        {
            var lista = repositorio.ObtenerTodos();
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