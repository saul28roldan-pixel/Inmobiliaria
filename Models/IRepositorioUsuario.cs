namespace Inmobiliaria.Models
{
    public interface IRepositorioUsuario
    {
        Usuario? ObtenerPorEmail(string email);
        Usuario? ObtenerPorId(int id);
    }
}