namespace Inmobiliaria.Models
{
    public interface IRepositorioInquilino
    {
        int Alta(Inquilino p);
        bool Baja(int id);
        bool Modificacion(Inquilino p);
        IList<Inquilino> ObtenerTodos();
        Inquilino? ObtenerPorId(int id);

        // Nuevo: búsqueda y paginado directamente en la BD
        IList<Inquilino> ObtenerFiltradosPaginados(
            string? busqueda,
            int pagina,
            int registrosPorPagina,
            out int totalRegistros
        );
    
    // Método para búsqueda AJAX con Select2
List<object> BuscarParaSelect(string termino, int maxResultados = 10);
}
}