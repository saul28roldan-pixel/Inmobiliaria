namespace Inmobiliaria.Models
{
    public interface IRepositorioPropietario
    {
        int Alta(Propietario p);
        bool Baja(int id);
        bool Modificacion(Propietario p);
        IList<Propietario> ObtenerTodos();
        Propietario? ObtenerPorId(int id);
        IList<Propietario> ObtenerFiltradosPaginados(
    string? busqueda,
    int pagina,
    int registrosPorPagina,
    out int totalRegistros
);
// Método para búsqueda AJAX con Select2 (devuelve solo id y texto)
List<object> BuscarParaSelect(string termino, int maxResultados = 10);
    }
    
}