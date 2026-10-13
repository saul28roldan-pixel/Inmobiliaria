using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    [Authorize] 
    public class ReservasController : Controller
    {
        private readonly IRepositorioReserva _repoReserva;
        private readonly IRepositorioInquilino _repoInquilino;
        private readonly IRepositorioInmueble _repoInmueble;
        private readonly IRepositorioPago _repoPago; // ✅ Para crear el pago de la multa

        public ReservasController(
            IRepositorioReserva repoReserva, 
            IRepositorioInquilino repoInquilino,
            IRepositorioInmueble repoInmueble,
            IRepositorioPago repoPago) // ✅ Inyectado
        {
            _repoReserva = repoReserva;
            _repoInquilino = repoInquilino;
            _repoInmueble = repoInmueble;
            _repoPago = repoPago; // ✅ Asignado
        }

        public IActionResult Index(string? busqueda, int pagina = 1)
        {
            int registrosPorPagina = 10;
            var lista = _repoReserva.ObtenerFiltradosPaginados(busqueda, pagina, registrosPorPagina, out int totalRegistros);

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

        // ========================================================================
        // ✅ FINALIZAR RESERVA ANTICIPADAMENTE (CON CÁLCULO DE MULTA)
        // ========================================================================
        
        // GET: Reservas/FinalizarAnticipadamente/5
        public IActionResult FinalizarAnticipadamente(int id)
        {
            var reserva = _repoReserva.ObtenerPorId(id);
            if (reserva == null) return NotFound();

            if (reserva.FechaFinalizacion.HasValue)
            {
                TempData["Error"] = "Esta reserva ya fue finalizada anteriormente.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var hoy = DateTime.Today;
            var fechaPropuesta = hoy > reserva.FechaHasta ? reserva.FechaHasta : hoy;

            var tiempoOriginal = (reserva.FechaHasta - reserva.FechaDesde).TotalDays;
            var tiempoCumplido = (fechaPropuesta - reserva.FechaDesde).TotalDays;
            var tiempoRestante = tiempoOriginal - tiempoCumplido;

            decimal multa = 0;
            if (tiempoRestante > 0)
            {
                if (tiempoCumplido < (tiempoOriginal / 2))
                {
                    multa = (decimal)(tiempoRestante * (double)reserva.MontoDiario * 0.50);
                }
                else
                {
                    multa = (decimal)(tiempoRestante * (double)reserva.MontoDiario * 0.25);
                }
            }

            ViewBag.MultaCalculada = multa;
            ViewBag.FechaPropuesta = fechaPropuesta.ToString("yyyy-MM-dd");
            
            return View(reserva);
        }

        // POST: Reservas/FinalizarAnticipadamente/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FinalizarAnticipadamente(int id, DateTime fechaFinalizacion)
        {
            var reserva = _repoReserva.ObtenerPorId(id);
            if (reserva == null) return NotFound();

            if (reserva.FechaFinalizacion.HasValue)
            {
                TempData["Error"] = "Esta reserva ya fue finalizada.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            if (fechaFinalizacion < reserva.FechaDesde)
            {
                ViewBag.Error = "La fecha de finalización no puede ser anterior a la fecha de inicio.";
                ViewBag.MultaCalculada = 0;
                ViewBag.FechaPropuesta = fechaFinalizacion.ToString("yyyy-MM-dd");
                return View(reserva);
            }

            var tiempoOriginal = (reserva.FechaHasta - reserva.FechaDesde).TotalDays;
            var tiempoCumplido = (fechaFinalizacion - reserva.FechaDesde).TotalDays;
            var tiempoRestante = tiempoOriginal - tiempoCumplido;

            decimal multa = 0;
            if (tiempoRestante > 0)
            {
                if (tiempoCumplido < (tiempoOriginal / 2))
                {
                    multa = (decimal)(tiempoRestante * (double)reserva.MontoDiario * 0.50);
                }
                else
                {
                    multa = (decimal)(tiempoRestante * (double)reserva.MontoDiario * 0.25);
                }
            }

            try
            {
                int idUsuario = ObtenerIdUsuarioActual();

                // 1. Finalizar la reserva en la BD
                _repoReserva.FinalizarAnticipadamente(id, fechaFinalizacion, multa, idUsuario);

                // 2. Crear automáticamente el pago de la multa
                if (multa > 0)
                {
                    var pagoMulta = new Pago
                    {
                        IdReserva = id,
                        IdUsuarioCreacion = idUsuario,
                        Concepto = "Multa por finalización anticipada",
                        FechaPago = DateTime.Today,
                        Importe = multa,
                        Anulado = false
                    };
                    _repoPago.Alta(pagoMulta);
                }

                TempData["Mensaje"] = "Reserva finalizada correctamente. Se registró el pago de la multa automáticamente.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo finalizar la reserva: " + ex.Message;
                ViewBag.MultaCalculada = multa;
                ViewBag.FechaPropuesta = fechaFinalizacion.ToString("yyyy-MM-dd");
                return View(reserva);
            }
        }

        // ========================================================================

        private int ObtenerIdUsuarioActual()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }
    }
}