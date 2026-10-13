using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly IRepositorioPago _repoPago;
        private readonly IRepositorioReserva _repoReserva;

        public PagosController(
            IRepositorioPago repoPago,
            IRepositorioReserva repoReserva)
        {
            _repoPago = repoPago;
            _repoReserva = repoReserva;
        }

        // GET: Pagos/Index/5 (lista pagos de una reserva específica)
        public IActionResult Index(int idReserva)
        {
            var reserva = _repoReserva.ObtenerPorId(idReserva);
            if (reserva == null)
            {
                return NotFound();
            }

            var pagos = _repoPago.ObtenerPorReserva(idReserva);
            ViewBag.Reserva = reserva;
            return View(pagos);
        }

        // GET: Pagos/Create/5 (formulario para crear pago de una reserva)
        public IActionResult Create(int idReserva)
        {
            var reserva = _repoReserva.ObtenerPorId(idReserva);
            if (reserva == null)
            {
                return NotFound();
            }

            ViewBag.IdReserva = idReserva;
            return View();
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pago pago)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IdReserva = pago.IdReserva;
                return View(pago);
            }

            try
            {
                pago.IdUsuarioCreacion = ObtenerIdUsuarioActual();
                pago.Anulado = false;
                _repoPago.Alta(pago);
                TempData["Mensaje"] = "Pago registrado correctamente.";
                return RedirectToAction(nameof(Index), new { idReserva = pago.IdReserva });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo crear el pago: " + ex.Message;
                ViewBag.IdReserva = pago.IdReserva;
                return View(pago);
            }
        }

        // GET: Pagos/Details/5
        public IActionResult Details(int id)
        {
            var pago = _repoPago.ObtenerPorId(id);
            if (pago == null)
            {
                return NotFound();
            }
            return View(pago);
        }

        // GET: Pagos/Edit/5 (solo permite editar el concepto)
        public IActionResult Edit(int id)
        {
            var pago = _repoPago.ObtenerPorId(id);
            if (pago == null || pago.Anulado)
            {
                return NotFound();
            }
            return View(pago);
        }

        // POST: Pagos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, string concepto)
        {
            var pago = _repoPago.ObtenerPorId(id);
            if (pago == null || pago.Anulado)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(concepto))
            {
                ViewBag.Error = "El concepto no puede estar vacío.";
                return View(pago);
            }

            try
            {
                _repoPago.ModificacionConcepto(id, concepto);
                TempData["Mensaje"] = "Concepto actualizado correctamente.";
                return RedirectToAction(nameof(Index), new { idReserva = pago.IdReserva });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo actualizar el concepto: " + ex.Message;
                return View(pago);
            }
        }

        // POST: Pagos/Anular/5 (cambia estado a anulado)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Anular(int id)
        {
            var pago = _repoPago.ObtenerPorId(id);
            if (pago == null || pago.Anulado)
            {
                return NotFound();
            }

            try
            {
                int idUsuario = ObtenerIdUsuarioActual();
                _repoPago.Anular(id, idUsuario);
                TempData["Mensaje"] = "Pago anulado correctamente.";
                return RedirectToAction(nameof(Index), new { idReserva = pago.IdReserva });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo anular el pago: " + ex.Message;
                return RedirectToAction(nameof(Index), new { idReserva = pago.IdReserva });
            }
        }

        // Helper: obtener el ID del usuario logueado
        private int ObtenerIdUsuarioActual()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }
    }
}