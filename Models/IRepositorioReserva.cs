using System;
using System.Collections.Generic;

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
        
        // Nuevo método para filtrar en la base de datos con paginado
        List<Reserva> ObtenerFiltradosPaginados(string? busqueda, int pagina, int registrosPorPagina, out int totalRegistros);
    }
}