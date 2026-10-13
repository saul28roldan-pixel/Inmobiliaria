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
        private readonly IRepositorioPago _repoPago; 

        public ReservasController(
            IRepositorioReserva repoReserva, 
            IRepositorioInquilino repoInquilino,
            IRepositorioInmueble repoInmueble,
            IRepositorioPago repoPago) 
        {
            _repoReserva = repoReserva;
            _repoInquilino = repoInquilino;
            _repoInmueble = repoInmueble;
            _repoPago = repoPago; 
        }

public IActionResult Create()
{
    return View(new Reserva());
}

        public IActionResult Index(string? busqueda, int pagina = 1)
        {
            if (pagina < 1) pagina = 1;
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

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Reserva reserva)
        {
            reserva.IdUsuarioCreacion = ObtenerIdUsuarioActual();
            ValidarReserva(reserva);

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
            CompletarNombres(reserva);
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
            ValidarReserva(reserva);

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
            CompletarNombres(reserva);
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "administrador")]
        public IActionResult Delete(int id)
        {
            try
            {
                _repoReserva.Eliminar(id);
                TempData["Mensaje"] = "Reserva eliminada.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No se puede eliminar la reserva: tiene pagos asociados.";
            }
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

            if (fechaFinalizacion.Date > reserva.FechaHasta.Date)
            {
                ViewBag.Error = "La fecha de finalización no puede ser posterior a la fecha de fin original de la reserva.";
                ViewBag.MultaCalculada = 0;
                ViewBag.FechaPropuesta = reserva.FechaHasta.ToString("yyyy-MM-dd");
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
        // RENOVAR / EXTENDER RESERVA
        // No modifica la reserva original: crea una reserva NUEVA con el mismo
        // inquilino e inmueble, nuevas fechas y nuevo monto diario.
        // ========================================================================

        // GET: Reservas/Renovar/5
        public IActionResult Renovar(int id)
        {
            var original = _repoReserva.ObtenerPorId(id);
            if (original == null) return NotFound();

            if (original.FechaFinalizacion.HasValue)
            {
                TempData["Error"] = "No se puede renovar una reserva que fue finalizada anticipadamente.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Sugerencia: arranca el día siguiente al fin de la original, con la misma duración y monto.
            int dias = Math.Max((original.FechaHasta - original.FechaDesde).Days, 1);
            var desde = original.FechaHasta.Date.AddDays(1);

            var model = new RenovarReservaViewModel
            {
                IdReservaOriginal = original.IdReserva,
                FechaDesde = desde,
                FechaHasta = desde.AddDays(dias),
                MontoDiario = original.MontoDiario
            };
            CompletarDatosRenovacion(model, original);
            return View(model);
        }

        // POST: Reservas/Renovar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Renovar(RenovarReservaViewModel model)
        {
            // Siempre se parte de la reserva guardada en la BD (inquilino e inmueble no vienen del formulario).
            var original = _repoReserva.ObtenerPorId(model.IdReservaOriginal);
            if (original == null) return NotFound();

            if (original.FechaFinalizacion.HasValue)
            {
                TempData["Error"] = "No se puede renovar una reserva que fue finalizada anticipadamente.";
                return RedirectToAction(nameof(Details), new { id = original.IdReserva });
            }

            CompletarDatosRenovacion(model, original);

            if (model.FechaDesde.Date <= original.FechaHasta.Date)
            {
                ModelState.AddModelError(nameof(model.FechaDesde),
                    $"La renovación debe comenzar después del fin de la reserva actual ({original.FechaHasta:dd/MM/yyyy}).");
            }
            if (model.FechaHasta.Date <= model.FechaDesde.Date)
            {
                ModelState.AddModelError(nameof(model.FechaHasta),
                    "La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            if (!ModelState.IsValid) return View(model);

            try
            {
                var nueva = new Reserva
                {
                    IdInquilino = original.IdInquilino,
                    IdInmueble = original.IdInmueble,
                    IdUsuarioCreacion = ObtenerIdUsuarioActual(),
                    FechaDesde = model.FechaDesde.Date,
                    FechaHasta = model.FechaHasta.Date,
                    MontoDiario = model.MontoDiario
                };

                // Alta vuelve a verificar que el inmueble no esté ocupado en esas fechas.
                int idNueva = _repoReserva.Alta(nueva);

                TempData["Mensaje"] = $"Reserva renovada correctamente. Se creó la reserva #{idNueva} (la reserva #{original.IdReserva} no se modificó).";
                return RedirectToAction(nameof(Details), new { id = idNueva });
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
        }

        private static void CompletarDatosRenovacion(RenovarReservaViewModel model, Reserva original)
        {
            model.InquilinoNombre = original.Inquilino?.NombreCompleto;
            model.InmuebleDireccion = original.Inmueble?.Direccion;
            model.FechaDesdeOriginal = original.FechaDesde;
            model.FechaHastaOriginal = original.FechaHasta;
        }

        // ========================================================================

        // Reglas comunes de Create y Edit: fechas coherentes y monto positivo.
        // (La superposición con otras reservas la controla el repositorio.)
        private void ValidarReserva(Reserva reserva)
        {
            if (reserva.FechaDesde != default && reserva.FechaHasta != default
                && reserva.FechaHasta.Date <= reserva.FechaDesde.Date)
            {
                ModelState.AddModelError(nameof(reserva.FechaHasta),
                    "La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            if (reserva.MontoDiario <= 0)
            {
                ModelState.AddModelError(nameof(reserva.MontoDiario),
                    "El monto diario debe ser mayor a 0.");
            }
        }

        // Si el formulario vuelve con errores, recupera los nombres para que los
        // desplegables (Select2) mantengan el inquilino/inmueble elegido.
        private void CompletarNombres(Reserva reserva)
        {
            if (reserva.IdInquilino > 0)
            {
                var inq = _repoInquilino.ObtenerPorId(reserva.IdInquilino);
                if (inq != null) reserva.Inquilino = new Inquilino { NombreCompleto = inq.NombreCompleto };
            }
            if (reserva.IdInmueble > 0)
            {
                var inm = _repoInmueble.ObtenerPorId(reserva.IdInmueble);
                if (inm != null) reserva.Inmueble = new Inmueble { Direccion = inm.Direccion };
            }
        }

        private int ObtenerIdUsuarioActual()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }
    }
}