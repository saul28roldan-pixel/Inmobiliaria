using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public interface IRepositorioTipoInmueble
    {
        int Alta(TipoInmueble t);
        bool Baja(int id);
        bool Modificacion(TipoInmueble t);
        IList<TipoInmueble> ObtenerTodos();
        TipoInmueble? ObtenerPorId(int id);
        
        // Nuevo método para filtrar con paginado
        IList<TipoInmueble> ObtenerFiltradosPaginados(string? busqueda, int pagina, int registrosPorPagina, out int totalRegistros);
    }
}