using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class CambiarClaveViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        public string ClaveActual { get; set; } = "";

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string ClaveNueva { get; set; } = "";

        [Required(ErrorMessage = "Debés confirmar la nueva contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nueva Contraseña")]
        [Compare("ClaveNueva", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarClave { get; set; } = "";
    }
}