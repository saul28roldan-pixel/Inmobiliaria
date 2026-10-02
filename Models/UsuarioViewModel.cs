using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class UsuarioViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        [Display(Name = "Email (Usuario)")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = "";

        [Required(ErrorMessage = "El rol es obligatorio")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = "empleado";

        [Display(Name = "Avatar (URL)")]
        public string? Avatar { get; set; }
    }
}