using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public interface IRepositorioPago
    {
        int Alta(Pago p);
        bool Anular(int idPago, int idUsuarioAnulacion);
        Pago? ObtenerPorId(int id);
        IList<Pago> ObtenerPorReserva(int idReserva);
        bool ModificacionConcepto(int idPago, string nuevoConcepto);
    }
}