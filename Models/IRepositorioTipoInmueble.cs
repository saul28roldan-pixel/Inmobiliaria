namespace Inmobiliaria.Models
{
    public interface IRepositorioTipoInmueble
    {
        int Alta(TipoInmueble t);
        bool Baja(int id);
        bool Modificacion(TipoInmueble t);
        IList<TipoInmueble> ObtenerTodos();
        TipoInmueble? ObtenerPorId(int id);
    }
}