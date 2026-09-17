using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    // ✅ REQUISITO DEL PDF: Protege todo el controlador requiriendo autenticación
    [Authorize] 
    public class ReservasController : Controller
    {
        private readonly IRepositorioReserva _repoReserva;
        private readonly IRepositorioInquilino _repoInquilino;
        private readonly IRepositorioInmueble _repoInmueble;

        public ReservasController(
            IRepositorioReserva repoReserva, 
            IRepositorioInquilino repoInquilino,
            IRepositorioInmueble repoInmueble)
        {
            _repoReserva = repoReserva;
            _repoInquilino = repoInquilino;
            _repoInmueble = repoInmueble;
        }

        public IActionResult Index()
        {
            var lista = _repoReserva.ObtenerTodos();
            return View(lista);
        }

        // ✅ NUEVO: Acción de Informe para cumplir con el requisito de la narrativa
        public IActionResult Informe()
        {
            var todasLasReservas = _repoReserva.ObtenerTodos();
            
            // Filtramos reservas que aún no han finalizado (lógica básica de reporte)
            var reservasActivas = todasLasReservas.Where(r => r.FechaHasta >= System.DateTime.Today).ToList();
            
            // Cálculos para el informe
            ViewBag.TotalReservasActivas = reservasActivas.Count;
            ViewBag.MontoTotalEstimado = reservasActivas.Sum(r => r.MontoDiario * (r.FechaHasta - r.FechaDesde).Days);

            return View(reservasActivas);
        }

        public IActionResult Create()
        {
            CargarDesplegables();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Reserva reserva)
        {
            reserva.IdUsuarioCreacion = 1; 
            if (ModelState.IsValid)
            {
                _repoReserva.Alta(reserva);
                TempData["Mensaje"] = "Reserva creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            CargarDesplegables();
            return View(reserva);
        }

        public IActionResult Details(int id)
        {
            var reserva = _repoReserva.ObtenerPorId(id);
            if (reserva == null) return NotFound();
            return View(reserva);
        }

        public IActionResult Edit(int id)
        {
            var reserva = _repoReserva.ObtenerPorId(id);
            if (reserva == null) return NotFound();
            CargarDesplegables();
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Reserva reserva)
        {
            if (id != reserva.IdReserva) return BadRequest();
            if (ModelState.IsValid)
            {
                _repoReserva.Modificacion(reserva);
                TempData["Mensaje"] = "Reserva actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            CargarDesplegables();
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _repoReserva.Eliminar(id);
            TempData["Mensaje"] = "Reserva eliminada.";
            return RedirectToAction(nameof(Index));
        }

        private void CargarDesplegables()
        {
            ViewBag.Inquilinos = new SelectList(_repoInquilino.ObtenerTodos(), "IdInquilino", "NombreCompleto");
            ViewBag.Inmuebles = new SelectList(_repoInmueble.ObtenerTodos(), "IdInmueble", "Direccion");
        }
    }
}