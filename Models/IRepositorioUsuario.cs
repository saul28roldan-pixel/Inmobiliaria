using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public interface IRepositorioUsuario
    {
        List<Usuario> ObtenerTodos();
        Usuario? ObtenerPorEmail(string email);
        Usuario? ObtenerPorId(int id);
        void Alta(Usuario usuario);
        void Modificacion(Usuario usuario);
        void Baja(int id);
    }
}