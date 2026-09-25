using Microsoft.AspNetCore.Authorization; // Agregado para [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inmobiliaria.Models;
using System.Linq;

namespace Inmobiliaria.Controllers
{
    [Authorize] // Agregado: Protege todo el controlador
    public class InmueblesController : Controller
    {
        private readonly IRepositorioInmueble repositorioInmueble;
        private readonly IRepositorioPropietario repositorioPropietario;
        private readonly IRepositorioTipoInmueble repositorioTipoInmueble;
        private readonly IRepositorioImagenInmueble repositorioImagenInmueble;
        private readonly IWebHostEnvironment entorno;

        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long TamanioMaximoBytes = 5 * 1024 * 1024; // 5 MB

        public InmueblesController(IRepositorioInmueble repositorioInmueble, IRepositorioPropietario repositorioPropietario, IRepositorioTipoInmueble repositorioTipoInmueble, IRepositorioImagenInmueble repositorioImagenInmueble, IWebHostEnvironment entorno)
        {
            this.repositorioInmueble = repositorioInmueble;
            this.repositorioPropietario = repositorioPropietario;
            this.repositorioTipoInmueble = repositorioTipoInmueble;
            this.repositorioImagenInmueble = repositorioImagenInmueble;
            this.entorno = entorno;
        }

        // GET: Inmuebles (CON BÚSQUEDA)
        public IActionResult Index(string? busqueda)
        {
            var lista = repositorioInmueble.ObtenerTodos();

            // Si el usuario escribió algo en el buscador, filtramos la lista
                        if (!string.IsNullOrEmpty(busqueda))
            {
                lista = lista.Where(i => 
                    i.Direccion.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(i.NombrePropietario) && i.NombrePropietario.Contains(busqueda, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            // Guardamos el término de búsqueda para que el input no se borre al recargar
            ViewBag.BusquedaActual = busqueda;
            
            return View(lista);
        }

        // GET: Inmuebles/Details/5
        public IActionResult Details(int id)
        {
            var i = repositorioInmueble.ObtenerPorId(id);
            if (i == null)
            {
                return NotFound();
            }
            return View(i);
        }

        // GET: Inmuebles/Create
        public IActionResult Create()
        {
            CargarDropdowns();
            return View();
        }

                // POST: Inmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Inmueble i)
        {
            if (!ModelState.IsValid)
            {
                CargarDropdowns();
                return View(i);
            }

            try
            {
                // 1. Guardamos el inmueble y capturamos el nuevo ID (si el repositorio lo devuelve)
                int nuevoId = repositorioInmueble.Alta(i);
                
                // 2. Aseguramos que el objeto tenga el ID correcto (por si el repositorio no lo actualizó por referencia)
                if (nuevoId > 0) 
                {
                    i.IdInmueble = nuevoId;
                }

                   var rutaImagen = GuardarImagen(i.ImagenFile);
                i.ImagenPortada = rutaImagen;
                
                // Actualizamos la portada en la base de datos
                repositorioInmueble.Modificacion(i); 

                // 3. Si cargaron portada al crear, queda también como la primera foto de la galería
                if (!string.IsNullOrEmpty(rutaImagen))
                {
                    repositorioImagenInmueble.Alta(new ImagenInmueble
                    {
                        IdInmueble = i.IdInmueble,
                        RutaUrl = rutaImagen,
                        EsPortada = true
                    });
                }

                TempData["Mensaje"] = "Inmueble creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo crear el inmueble: " + ex.Message;
                CargarDropdowns();
                return View(i);
            }
        }

        // GET: Inmuebles/Edit/5
        public IActionResult Edit(int id)
        {
            var i = repositorioInmueble.ObtenerPorId(id);
            if (i == null)
            {
                return NotFound();
            }
            CargarDropdowns();
            return View(i);
        }

        // POST: Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Inmueble i)
        {
            if (id != i.IdInmueble)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                CargarDropdowns();
                return View(i);
            }

            try
            {
                var existente = repositorioInmueble.ObtenerPorId(id);
                if (existente == null)
                {
                    return NotFound();
                }

                if (i.ImagenFile != null && i.ImagenFile.Length > 0)
                {
                    // Subieron una imagen nueva: se suma a la galería como
                    // nueva portada. La foto anterior NO se borra: solo deja
                    // de ser portada, pero se conserva como foto del inmueble.
                    var nuevaRuta = GuardarImagen(i.ImagenFile);
                    i.ImagenPortada = nuevaRuta;

                    var nuevaImagen = new ImagenInmueble
                    {
                        IdInmueble = i.IdInmueble,
                        RutaUrl = nuevaRuta!,
                        EsPortada = false
                    };
                    int idNuevaImagen = repositorioImagenInmueble.Alta(nuevaImagen);
                    repositorioImagenInmueble.MarcarComoPortada(idNuevaImagen, i.IdInmueble);
                }
                else
                {
                    // No tocaron el campo de imagen: se conserva la que ya tenía
                    i.ImagenPortada = existente.ImagenPortada;
                }

                repositorioInmueble.Modificacion(i);
                TempData["Mensaje"] = "Inmueble modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "No se pudo modificar el inmueble: " + ex.Message;
                CargarDropdowns();
                return View(i);
            }
        }

        // GET: Inmuebles/Delete/5
        public IActionResult Delete(int id)
        {
            var i = repositorioInmueble.ObtenerPorId(id);
            if (i == null)
            {
                return NotFound();
            }
            return View(i);
        }

        // POST: Inmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                // El FK de inmuebleimagen tiene ON DELETE CASCADE, así que las
                // filas se borran solas; pero los archivos físicos no, por eso
                // los borramos nosotros antes de perder la referencia.
                var imagenes = repositorioImagenInmueble.ObtenerPorInmueble(id);
                repositorioInmueble.Baja(id);

                foreach (var img in imagenes)
                {
                    EliminarImagenSiExiste(img.RutaUrl);
                }

                TempData["Mensaje"] = "Inmueble eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo eliminar el inmueble: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Inmuebles/Imagenes/5  (galería de fotos de un inmueble)
        public IActionResult Imagenes(int id)
        {
            var inmueble = repositorioInmueble.ObtenerPorId(id);
            if (inmueble == null)
            {
                return NotFound();
            }

            ViewBag.Inmueble = inmueble;
            var imagenes = repositorioImagenInmueble.ObtenerPorInmueble(id);
            return View(imagenes);
        }

        // POST: Inmuebles/AgregarImagenes (subida múltiple para la galería)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarImagenes(int idInmueble, List<IFormFile> archivos)
        {
            var inmueble = repositorioInmueble.ObtenerPorId(idInmueble);
            if (inmueble == null)
            {
                return NotFound();
            }

            if (archivos == null || archivos.Count == 0 || archivos.All(a => a.Length == 0))
            {
                TempData["Error"] = "Seleccioná al menos una imagen para subir.";
                return RedirectToAction(nameof(Imagenes), new { id = idInmueble });
            }

            try
            {
                bool yaTienePortada = repositorioImagenInmueble.ObtenerPorInmueble(idInmueble).Any(im => im.EsPortada);

                foreach (var archivo in archivos)
                {
                    if (archivo.Length == 0) continue;

                    var ruta = GuardarImagen(archivo);
                    if (ruta == null) continue;

                    bool esPrimeraPortada = !yaTienePortada;
                    repositorioImagenInmueble.Alta(new ImagenInmueble
                    {
                        IdInmueble = idInmueble,
                        RutaUrl = ruta,
                        EsPortada = esPrimeraPortada
                    });

                    // La primera foto que se sube (si el inmueble no tenía
                    // ninguna) queda como portada automáticamente.
                    if (esPrimeraPortada)
                    {
                        inmueble.ImagenPortada = ruta;
                        repositorioInmueble.Modificacion(inmueble);
                        yaTienePortada = true;
                    }
                }

                TempData["Mensaje"] = "Imágenes agregadas correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudieron subir las imágenes: " + ex.Message;
            }

            return RedirectToAction(nameof(Imagenes), new { id = idInmueble });
        }

        // POST: Inmuebles/MarcarPortada
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarPortada(int idImagen, int idInmueble)
        {
            var imagen = repositorioImagenInmueble.ObtenerPorId(idImagen);
            var inmueble = repositorioInmueble.ObtenerPorId(idInmueble);
            if (imagen == null || inmueble == null || imagen.IdInmueble != idInmueble)
            {
                return NotFound();
            }

            repositorioImagenInmueble.MarcarComoPortada(idImagen, idInmueble);
            inmueble.ImagenPortada = imagen.RutaUrl;
            repositorioInmueble.Modificacion(inmueble);

            TempData["Mensaje"] = "Portada actualizada.";
            return RedirectToAction(nameof(Imagenes), new { id = idInmueble });
        }

        // POST: Inmuebles/EliminarImagen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarImagen(int idImagen, int idInmueble)
        {
            var imagen = repositorioImagenInmueble.ObtenerPorId(idImagen);
            if (imagen == null || imagen.IdInmueble != idInmueble)
            {
                return NotFound();
            }

            try
            {
                bool eraPortada = imagen.EsPortada;
                repositorioImagenInmueble.Baja(idImagen);
                EliminarImagenSiExiste(imagen.RutaUrl);

                if (eraPortada)
                {
                    // Si borraron la portada, promovemos otra foto que haya
                    // quedado (si hay), o dejamos el inmueble sin portada.
                    var inmueble = repositorioInmueble.ObtenerPorId(idInmueble);
                    if (inmueble != null)
                    {
                        var nuevaPortada = repositorioImagenInmueble.ObtenerPorInmueble(idInmueble).FirstOrDefault();
                        if (nuevaPortada != null)
                        {
                            repositorioImagenInmueble.MarcarComoPortada(nuevaPortada.IdImagen, idInmueble);
                            inmueble.ImagenPortada = nuevaPortada.RutaUrl;
                        }
                        else
                        {
                            inmueble.ImagenPortada = null;
                        }
                        repositorioInmueble.Modificacion(inmueble);
                    }
                }

                TempData["Mensaje"] = "Imagen eliminada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo eliminar la imagen: " + ex.Message;
            }

            return RedirectToAction(nameof(Imagenes), new { id = idInmueble });
        }

        // Guarda el archivo subido en wwwroot/uploads/inmuebles y devuelve
        // la ruta relativa que se persiste en ImagenPortada. Devuelve null
        // si no llegó ningún archivo.
        private string? GuardarImagen(IFormFile? archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                throw new Exception("Formato de imagen no permitido. Use JPG, PNG, WEBP o GIF.");
            }

            if (archivo.Length > TamanioMaximoBytes)
            {
                throw new Exception("La imagen no puede superar los 5 MB.");
            }

            var carpetaDestino = Path.Combine(entorno.WebRootPath, "uploads", "inmuebles");
            Directory.CreateDirectory(carpetaDestino);

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaFisica = Path.Combine(carpetaDestino, nombreArchivo);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                archivo.CopyTo(stream);
            }

            // Ruta relativa (web) que se guarda en la base y se usa en las vistas
            return $"/uploads/inmuebles/{nombreArchivo}";
        }

        // Borra del disco la imagen anterior, si existe y sigue dentro de wwwroot.
        private void EliminarImagenSiExiste(string? rutaRelativa)
        {
            if (string.IsNullOrEmpty(rutaRelativa))
            {
                return;
            }

            var rutaFisica = Path.Combine(entorno.WebRootPath, rutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }
        }

        // Método privado para cargar los dropdowns de Propietario y TipoInmueble
        private void CargarDropdowns()
        {
            var propietarios = repositorioPropietario.ObtenerTodos();
            var listaPropietarios = propietarios.Select(p => new SelectListItem
            {
                Value = p.IdPropietario.ToString(),
                Text = $"{p.Nombre} {p.Apellido}"
            }).ToList();
            ViewBag.Propietarios = listaPropietarios;

            var tipos = repositorioTipoInmueble.ObtenerTodos();
            ViewBag.Tipos = tipos.Select(t => new SelectListItem
            {
                Value = t.IdTipo.ToString(),
                Text = t.Descripcion
            }).ToList();
        }
    }
}