using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public class RepositorioInmueble : RepositorioBase, IRepositorioInmueble
    {
        // Constructor corregido: recibe IConfiguration y se lo pasa a la clase base
        public RepositorioInmueble(IConfiguration configuration) : base(configuration) 
        { 
        }

        public int Alta(Inmueble i)
        {
            int id = 0;
            string sql = @"INSERT INTO Inmueble (IdPropietario, IdTipo, Direccion, Cupo, Coordenadas, PrecioPorDia, ImagenPortada, Disponible)
                            VALUES (@idPropietario, @idTipo, @direccion, @cupo, @coordenadas, @precioPorDia, @imagenPortada, @disponible);
                            SELECT LAST_INSERT_ID();";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@idPropietario", i.IdPropietario);
                command.Parameters.AddWithValue("@idTipo", i.IdTipo);
                command.Parameters.AddWithValue("@direccion", i.Direccion);
                command.Parameters.AddWithValue("@cupo", i.Cupo);
                command.Parameters.AddWithValue("@coordenadas", (object?)i.Coordenadas ?? DBNull.Value);
                command.Parameters.AddWithValue("@precioPorDia", i.PrecioPorDia);
                command.Parameters.AddWithValue("@imagenPortada", (object?)i.ImagenPortada ?? DBNull.Value);
                command.Parameters.AddWithValue("@disponible", i.Disponible);

                connection.Open();
                id = Convert.ToInt32(command.ExecuteScalar());
            }
            i.IdInmueble = id;
            return id;
        }

        public bool Baja(int id)
        {
            int filasAfectadas = 0;
            string sql = "DELETE FROM Inmueble WHERE IdInmueble = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }
            return filasAfectadas > 0;
        }

        public bool Modificacion(Inmueble i)
        {
            int filasAfectadas = 0;
            string sql = @"UPDATE Inmueble
                            SET IdPropietario = @idPropietario,
                                IdTipo = @idTipo,
                                Direccion = @direccion,
                                Cupo = @cupo,
                                Coordenadas = @coordenadas,
                                PrecioPorDia = @precioPorDia,
                                ImagenPortada = @imagenPortada,
                                Disponible = @disponible
                            WHERE IdInmueble = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@idPropietario", i.IdPropietario);
                command.Parameters.AddWithValue("@idTipo", i.IdTipo);
                command.Parameters.AddWithValue("@direccion", i.Direccion);
                command.Parameters.AddWithValue("@cupo", i.Cupo);
                command.Parameters.AddWithValue("@coordenadas", (object?)i.Coordenadas ?? DBNull.Value);
                command.Parameters.AddWithValue("@precioPorDia", i.PrecioPorDia);
                command.Parameters.AddWithValue("@imagenPortada", (object?)i.ImagenPortada ?? DBNull.Value);
                command.Parameters.AddWithValue("@disponible", i.Disponible);
                command.Parameters.AddWithValue("@id", i.IdInmueble);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }
            return filasAfectadas > 0;
        }

        public IList<Inmueble> ObtenerTodos()
        {
            var lista = new List<Inmueble>();
            string sql = @"SELECT i.IdInmueble, i.IdPropietario, i.IdTipo, i.Direccion, i.Cupo, 
                                  i.Coordenadas, i.PrecioPorDia, i.ImagenPortada, i.Disponible,
                                  CONCAT(p.Nombre, ' ', p.Apellido) AS NombrePropietario, 
                                  t.Descripcion AS DescripcionTipo
                           FROM Inmueble i
                           LEFT JOIN Propietario p ON i.IdPropietario = p.IdPropietario
                           LEFT JOIN TipoInmueble t ON i.IdTipo = t.IdTipo
                           ORDER BY i.Direccion";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(MapearInmueble(reader));
                    }
                }
            }
            return lista;
        }

        public Inmueble? ObtenerPorId(int id)
        {
            Inmueble? i = null;
            string sql = @"SELECT i.IdInmueble, i.IdPropietario, i.IdTipo, i.Direccion, i.Cupo, 
                                  i.Coordenadas, i.PrecioPorDia, i.ImagenPortada, i.Disponible,
                                  CONCAT(p.Nombre, ' ', p.Apellido) AS NombrePropietario, 
                                  t.Descripcion AS DescripcionTipo
                           FROM Inmueble i
                           LEFT JOIN Propietario p ON i.IdPropietario = p.IdPropietario
                           LEFT JOIN TipoInmueble t ON i.IdTipo = t.IdTipo
                           WHERE i.IdInmueble = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        i = MapearInmueble(reader);
                    }
                }
            }
            return i;
        }

        private static Inmueble MapearInmueble(MySqlDataReader reader)
        {
            return new Inmueble
            {
                IdInmueble = reader.GetInt32("IdInmueble"),
                IdPropietario = reader.GetInt32("IdPropietario"),
                IdTipo = reader.GetInt32("IdTipo"),
                Direccion = reader.GetString("Direccion"),
                Cupo = reader.GetInt32("Cupo"),
                Coordenadas = reader.IsDBNull(reader.GetOrdinal("Coordenadas")) ? null : reader.GetString("Coordenadas"),
                PrecioPorDia = reader.GetDecimal("PrecioPorDia"),
                ImagenPortada = reader.IsDBNull(reader.GetOrdinal("ImagenPortada")) ? null : reader.GetString("ImagenPortada"),
                Disponible = reader.GetBoolean("Disponible"),
                NombrePropietario = reader.IsDBNull(reader.GetOrdinal("NombrePropietario")) ? "Desconocido" : reader.GetString("NombrePropietario"),
                DescripcionTipo = reader.IsDBNull(reader.GetOrdinal("DescripcionTipo")) ? "Desconocido" : reader.GetString("DescripcionTipo")
            };
        }
    }
}