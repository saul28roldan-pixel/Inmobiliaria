namespace Inmobiliaria.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        
        public string Email { get; set; } = "";
        
        public string PasswordHash { get; set; } = "";
        
        public string NombreCompleto { get; set; } = "";
        
        public string Rol { get; set; } = ""; // "administrador" o "empleado"
        
        public string? Avatar { get; set; }
    }
}