   using Microsoft.AspNetCore.Mvc;
   using Microsoft.AspNetCore.Mvc.Rendering;
   using Inmobiliaria.Models;

   namespace Inmobiliaria.Controllers
   {
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