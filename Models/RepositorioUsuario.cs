using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
    {
        public RepositorioUsuario(IConfiguration configuration) : base(configuration)
        {
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            Usuario? u = null;
            string sql = @"SELECT IdUsuario, Email, PasswordHash, NombreCompleto, Rol, Avatar
                            FROM Usuario
                            WHERE Email = @email";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@email", email);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        u = MapearUsuario(reader);
                    }
                }
            }
            return u;
        }

        public Usuario? ObtenerPorId(int id)
        {
            Usuario? u = null;
            string sql = @"SELECT IdUsuario, Email, PasswordHash, NombreCompleto, Rol, Avatar
                            FROM Usuario
                            WHERE IdUsuario = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        u = MapearUsuario(reader);
                    }
                }
            }
            return u;
        }

        private static Usuario MapearUsuario(MySqlDataReader reader)
        {
            return new Usuario
            {
                IdUsuario = reader.GetInt32("IdUsuario"),
                Email = reader.GetString("Email"),
                PasswordHash = reader.GetString("PasswordHash"),
                NombreCompleto = reader.GetString("NombreCompleto"),
                Rol = reader.GetString("Rol"),
                Avatar = reader.IsDBNull(reader.GetOrdinal("Avatar")) ? null : reader.GetString("Avatar")
            };
        }
    }
}