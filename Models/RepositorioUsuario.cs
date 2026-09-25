using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
    {
        public RepositorioUsuario(IConfiguration configuration) : base(configuration)
        {
        }

        public List<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            using (var connection = ObtenerConexion())
            {
                var sql = "SELECT IdUsuario, Email, PasswordHash, NombreCompleto, Rol, Avatar FROM Usuario;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearUsuario(reader));
                        }
                    }
                }
            }
            return lista;
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

        public void Alta(Usuario usuario)
        {
            using (var connection = ObtenerConexion())
            {
                var sql = @"INSERT INTO Usuario (Email, PasswordHash, NombreCompleto, Rol, Avatar) 
                            VALUES (@Email, @PasswordHash, @NombreCompleto, @Rol, @Avatar);";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Email", usuario.Email);
                    command.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash);
                    command.Parameters.AddWithValue("@NombreCompleto", usuario.NombreCompleto);
                    command.Parameters.AddWithValue("@Rol", usuario.Rol);
                    command.Parameters.AddWithValue("@Avatar", (object)usuario.Avatar ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Modificacion(Usuario usuario)
        {
            using (var connection = ObtenerConexion())
            {
                var sql = @"UPDATE Usuario SET Email = @Email, PasswordHash = @PasswordHash, 
                            NombreCompleto = @NombreCompleto, Rol = @Rol, Avatar = @Avatar 
                            WHERE IdUsuario = @IdUsuario;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@IdUsuario", usuario.IdUsuario);
                    command.Parameters.AddWithValue("@Email", usuario.Email);
                    command.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash);
                    command.Parameters.AddWithValue("@NombreCompleto", usuario.NombreCompleto);
                    command.Parameters.AddWithValue("@Rol", usuario.Rol);
                    command.Parameters.AddWithValue("@Avatar", (object)usuario.Avatar ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Baja(int id)
        {
            using (var connection = ObtenerConexion())
            {
                var sql = "DELETE FROM Usuario WHERE IdUsuario = @IdUsuario;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@IdUsuario", id);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
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