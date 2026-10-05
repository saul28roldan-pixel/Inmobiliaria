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
    }
}