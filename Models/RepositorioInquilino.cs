using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public class RepositorioInquilino : RepositorioBase, IRepositorioInquilino
    {
        // Constructor corregido: recibe IConfiguration y se lo pasa a la clase base
        public RepositorioInquilino(IConfiguration configuration) : base(configuration) 
        { 
        }

        public int Alta(Inquilino i)
        {
            int id = 0;
            string sql = @"INSERT INTO Inquilino (NombreCompleto, Dni, Email, Telefono)
                            VALUES (@nombreCompleto, @dni, @email, @telefono);
                            SELECT LAST_INSERT_ID();";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@nombreCompleto", i.NombreCompleto);
                command.Parameters.AddWithValue("@dni", i.Dni);
                command.Parameters.AddWithValue("@email", (object?)i.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@telefono", (object?)i.Telefono ?? DBNull.Value);

                connection.Open();
                id = Convert.ToInt32(command.ExecuteScalar());
            }
            i.IdInquilino = id;
            return id;
        }

        public bool Baja(int id)
        {
            int filasAfectadas = 0;
            string sql = "DELETE FROM Inquilino WHERE IdInquilino = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }
            return filasAfectadas > 0;
        }

        public bool Modificacion(Inquilino i)
        {
            int filasAfectadas = 0;
            string sql = @"UPDATE Inquilino
                            SET NombreCompleto = @nombreCompleto,
                                Dni = @dni,
                                Email = @email,
                                Telefono = @telefono
                            WHERE IdInquilino = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@nombreCompleto", i.NombreCompleto);
                command.Parameters.AddWithValue("@dni", i.Dni);
                command.Parameters.AddWithValue("@email", (object?)i.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@telefono", (object?)i.Telefono ?? DBNull.Value);
                command.Parameters.AddWithValue("@id", i.IdInquilino);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }
            return filasAfectadas > 0;
        }

        public IList<Inquilino> ObtenerTodos()
        {
            var lista = new List<Inquilino>();
            string sql = @"SELECT IdInquilino, NombreCompleto, Dni, Email, Telefono
                            FROM Inquilino
                            ORDER BY NombreCompleto";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(MapearInquilino(reader));
                    }
                }
            }
            return lista;
        }

        public Inquilino? ObtenerPorId(int id)
        {
            Inquilino? i = null;
            string sql = @"SELECT IdInquilino, NombreCompleto, Dni, Email, Telefono
                            FROM Inquilino
                            WHERE IdInquilino = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        i = MapearInquilino(reader);
                    }
                }
            }
            return i;
        }

        private static Inquilino MapearInquilino(MySqlDataReader reader)
        {
            return new Inquilino
            {
                IdInquilino = reader.GetInt32("IdInquilino"),
                NombreCompleto = reader.GetString("NombreCompleto"),
                Dni = reader.GetString("Dni"),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
            };
        }
    }
}