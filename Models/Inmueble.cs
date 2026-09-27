using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Inmobiliaria.Models
{
    public class Inmueble
    {
        public int IdInmueble { get; set; }

        [Required(ErrorMessage = "El propietario es obligatorio")]
        [Display(Name = "Propietario")]
        public int IdPropietario { get; set; }

        [Required(ErrorMessage = "El tipo de inmueble es obligatorio")]
        [Display(Name = "Tipo de Inmueble")]
        public int IdTipo { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(255, ErrorMessage = "La dirección no puede exceder los 255 caracteres")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = "";

        [Required(ErrorMessage = "El cupo es obligatorio")]
        [Range(1, 50, ErrorMessage = "El cupo debe ser entre 1 y 50 personas")]
        [Display(Name = "Cupo Máximo de Personas")]
        public int Cupo { get; set; }

        [StringLength(100)]
        [Display(Name = "Coordenadas (ej: -34.6037,-58.3816)")]
        public string? Coordenadas { get; set; }

        [Required(ErrorMessage = "El precio por día es obligatorio")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio por Día ($)")]
        [DataType(DataType.Currency)]
        public decimal PrecioPorDia { get; set; }

        [StringLength(255)]
        [Display(Name = "Imagen de Portada")]
        public string? ImagenPortada { get; set; }

        // No se persiste en la base: es el archivo que llega del formulario
        // al crear/editar. El controller lo procesa y arma ImagenPortada.
        [Display(Name = "Imagen de Portada")]
        public IFormFile? ImagenFile { get; set; }

        [Display(Name = "Disponible para Alquiler")]
        public bool Disponible { get; set; } = true;

        // Propiedades de navegación (opcionales, para mostrar nombres en lugar de IDs en las vistas)
        public string? NombrePropietario { get; set; }
        public string? DescripcionTipo { get; set; }
    }
}