using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class RenovarReservaViewModel
    {
        public int IdReservaOriginal { get; set; }

        // Solo informativos (se recompletan desde la BD en cada request)
        public string? InquilinoNombre { get; set; }
        public string? InmuebleDireccion { get; set; }
        public DateTime FechaDesdeOriginal { get; set; }
        public DateTime FechaHastaOriginal { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Nueva fecha desde")]
        public DateTime FechaDesde { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Nueva fecha hasta")]
        public DateTime FechaHasta { get; set; }

        [Required(ErrorMessage = "El monto diario es obligatorio.")]
        [Range(0.01, 100000000, ErrorMessage = "El monto diario debe ser mayor a 0.")]
        [Display(Name = "Monto diario ($)")]
        public decimal MontoDiario { get; set; }
    }
}
