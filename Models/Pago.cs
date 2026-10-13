using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class Pago
    {
        public int IdPago { get; set; }

        [Required(ErrorMessage = "La reserva es obligatoria.")]
        public int IdReserva { get; set; }

        [Required(ErrorMessage = "El usuario de creación es obligatorio.")]
        public int IdUsuarioCreacion { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio.")]
        [StringLength(255, ErrorMessage = "El concepto no puede superar los 255 caracteres.")]
        public string Concepto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de pago es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaPago { get; set; }

        [Required(ErrorMessage = "El importe es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El importe debe ser mayor a 0.")]
        public decimal Importe { get; set; }

        public bool Anulado { get; set; }

        public int? IdUsuarioAnulacion { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaAnulacion { get; set; }

        // Propiedades de navegación (opcionales, para mostrar datos relacionados)
        public Reserva? Reserva { get; set; }
        public Usuario? UsuarioCreacion { get; set; }
        public Usuario? UsuarioAnulacion { get; set; }
    }
}