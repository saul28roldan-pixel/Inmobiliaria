using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class Reserva
    {
        public int IdReserva { get; set; }

        [Required(ErrorMessage = "El inquilino es obligatorio.")]
        public int IdInquilino { get; set; }

        [Required(ErrorMessage = "El inmueble es obligatorio.")]
        public int IdInmueble { get; set; }

        public int IdUsuarioCreacion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaDesde { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaHasta { get; set; }

        [Required(ErrorMessage = "El monto diario es obligatorio.")]
        public decimal MontoDiario { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaFinalizacion { get; set; }

        public decimal? Multa { get; set; }

        public int? IdUsuarioFinalizacion { get; set; }

        public Inquilino? Inquilino { get; set; }
        public Inmueble? Inmueble { get; set; }
        public Usuario? UsuarioCreacion { get; set; }
        public Usuario? UsuarioFinalizacion { get; set; }
    }
}