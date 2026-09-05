using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public class RepositorioPropietario : RepositorioBase, IRepositorioPropietario
    {
        // Constructor corregido: recibe IConfiguration y se lo pasa a la clase base
        public RepositorioPropietario(IConfiguration configuration) : base(configuration) 
        { 
        }

        public int Alta(Propietario p)
        {
            int id = 0;
            string sql = @"INSERT INTO Propietario (Nombre, Apellido, Dni, Email, Telefono)
                            VALUES (@nombre, @apellido, @dni, @email, @telefono);
                            SELECT LAST_INSERT_ID();";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@nombre", p.Nombre);
                command.Parameters.AddWithValue("@apellido", p.Apellido);
                command.Parameters.AddWithValue("@dni", p.Dni);
                command.Parameters.AddWithValue("@email", (object?)p.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@telefono", (object?)p.Telefono ?? DBNull.Value);

                connection.Open();
                id = Convert.ToInt32(command.ExecuteScalar());
            }
            p.IdPropietario = id;
            return id;
        }

        public bool Baja(int id)
        {
            int filasAfectadas = 0;
            string sql = "DELETE FROM Propietario WHERE IdPropietario = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }
            return filasAfectadas > 0;
        }

        public bool Modificacion(Propietario p)
        {
            int filasAfectadas = 0;
            string sql = @"UPDATE Propietario
                            SET Nombre = @nombre,
                                Apellido = @apellido,
                                Dni = @dni,
                                Email = @email,
                                Telefono = @telefono
                            WHERE IdPropietario = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@nombre", p.Nombre);
                command.Parameters.AddWithValue("@apellido", p.Apellido);
                command.Parameters.AddWithValue("@dni", p.Dni);
                command.Parameters.AddWithValue("@email", (object?)p.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@telefono", (object?)p.Telefono ?? DBNull.Value);
                command.Parameters.AddWithValue("@id", p.IdPropietario);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }
            return filasAfectadas > 0;
        }

        public IList<Propietario> ObtenerTodos()
        {
            var lista = new List<Propietario>();
            string sql = @"SELECT IdPropietario, Nombre, Apellido, Dni, Email, Telefono
                            FROM Propietario
                            ORDER BY Apellido, Nombre";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(MapearPropietario(reader));
                    }
                }
            }
            return lista;
        }

        public Propietario? ObtenerPorId(int id)
        {
            Propietario? p = null;
            string sql = @"SELECT IdPropietario, Nombre, Apellido, Dni, Email, Telefono
                            FROM Propietario
                            WHERE IdPropietario = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        p = MapearPropietario(reader);
                    }
                }
            }
            return p;
        }

        private static Propietario MapearPropietario(MySqlDataReader reader)
        {
            return new Propietario
            {
                IdPropietario = reader.GetInt32("IdPropietario"),
                Nombre = reader.GetString("Nombre"),
                Apellido = reader.GetString("Apellido"),
                Dni = reader.GetString("Dni"),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
            };
        }
    }
}