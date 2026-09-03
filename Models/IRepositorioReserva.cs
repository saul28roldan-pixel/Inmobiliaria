using Inmobiliaria.Models;

namespace Inmobiliaria.Models
{
    public interface IRepositorioReserva
    {
        List<Reserva> ObtenerTodos();
        Reserva? ObtenerPorId(int id);
        int Alta(Reserva reserva);
        int Modificacion(Reserva reserva);
        int FinalizarAnticipadamente(int idReserva, DateTime fechaFinalizacion, decimal multa, int idUsuarioFinalizacion);
        int Eliminar(int id);
    }
}