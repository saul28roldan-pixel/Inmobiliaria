using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class ImagenInmueble
    {
        public int IdImagen { get; set; }

        public int IdInmueble { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Ruta de la imagen")]
        public string RutaUrl { get; set; } = "";

        [Display(Name = "Es portada")]
        public bool EsPortada { get; set; }
    }
}
