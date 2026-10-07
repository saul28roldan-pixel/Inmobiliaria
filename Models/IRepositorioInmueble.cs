using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public interface IRepositorioInmueble
    {
        int Alta(Inmueble i);
        bool Baja(int id);
        bool Modificacion(Inmueble i);
        IList<Inmueble> ObtenerTodos(); // Devolverá la lista con los nombres de propietario y tipo ya unidos
        Inmueble? ObtenerPorId(int id);
        
        // Nuevo método para filtrar en la base de datos con paginado
        IList<Inmueble> ObtenerFiltradosPaginados(string? busqueda, int? idTipo, int pagina, int registrosPorPagina, out int totalRegistros);
    
            // Método para búsqueda AJAX con Select2
        List<object> BuscarParaSelect(string termino, int maxResultados = 10);
}
}