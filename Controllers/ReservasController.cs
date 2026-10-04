using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    // ✅ AUTORIZACIÓN ACTIVADA: Requiere que el usuario esté logueado
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

        // GET: Reservas (CON BÚSQUEDA EN BD Y PAGINADO)
public IActionResult Index(string? busqueda, int pagina = 1)
{
    int registrosPorPagina = 10;

    var lista = _repoReserva.ObtenerFiltradosPaginados(
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

        public IActionResult Informe()
        {
            var todasLasReservas = _repoReserva.ObtenerTodos();
            var reservasActivas = todasLasReservas.Where(r => r.FechaHasta >= System.DateTime.Today).ToList();
            
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
            reserva.IdUsuarioCreacion = ObtenerIdUsuarioActual();

            if (ModelState.IsValid)
            {
                try
                {
                    _repoReserva.Alta(reserva);
                    TempData["Mensaje"] = "Reserva creada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                }
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
                try
                {
                    _repoReserva.Modificacion(reserva);
                    TempData["Mensaje"] = "Reserva actualizada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                }
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

        private int ObtenerIdUsuarioActual()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }
    }
}