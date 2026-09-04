using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class TipoInmueble
    {
        public int IdTipo { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = "";
    }
}