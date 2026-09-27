using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public class RepositorioImagenInmueble : RepositorioBase, IRepositorioImagenInmueble
    {
        public RepositorioImagenInmueble(IConfiguration configuration) : base(configuration)
        {
        }

        public int Alta(ImagenInmueble img)
        {
            int id = 0;
            string sql = @"INSERT INTO inmuebleimagen (IdInmueble, RutaUrl, EsPortada)
                            VALUES (@idInmueble, @rutaUrl, @esPortada);
                            SELECT LAST_INSERT_ID();";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@idInmueble", img.IdInmueble);
                command.Parameters.AddWithValue("@rutaUrl", img.RutaUrl);
                command.Parameters.AddWithValue("@esPortada", img.EsPortada);

                connection.Open();
                id = Convert.ToInt32(command.ExecuteScalar());
            }
            img.IdImagen = id;
            return id;
        }

        public bool Baja(int idImagen)
        {
            int filasAfectadas = 0;
            string sql = "DELETE FROM inmuebleimagen WHERE IdImagen = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", idImagen);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }
            return filasAfectadas > 0;
        }

        public ImagenInmueble? ObtenerPorId(int idImagen)
        {
            ImagenInmueble? img = null;
            string sql = "SELECT IdImagen, IdInmueble, RutaUrl, EsPortada FROM inmuebleimagen WHERE IdImagen = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", idImagen);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        img = Mapear(reader);
                    }
                }
            }
            return img;
        }

        public IList<ImagenInmueble> ObtenerPorInmueble(int idInmueble)
        {
            var lista = new List<ImagenInmueble>();
            string sql = @"SELECT IdImagen, IdInmueble, RutaUrl, EsPortada
                           FROM inmuebleimagen
                           WHERE IdInmueble = @idInmueble
                           ORDER BY EsPortada DESC, IdImagen ASC";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@idInmueble", idInmueble);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(Mapear(reader));
                    }
                }
            }
            return lista;
        }

        public bool MarcarComoPortada(int idImagen, int idInmueble)
        {
            using (var connection = ObtenerConexion())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var limpiar = new MySqlCommand(
                        "UPDATE inmuebleimagen SET EsPortada = 0 WHERE IdInmueble = @idInmueble",
                        connection, transaction);
                    limpiar.Parameters.AddWithValue("@idInmueble", idInmueble);
                    limpiar.ExecuteNonQuery();

                    var marcar = new MySqlCommand(
                        "UPDATE inmuebleimagen SET EsPortada = 1 WHERE IdImagen = @idImagen AND IdInmueble = @idInmueble",
                        connection, transaction);
                    marcar.Parameters.AddWithValue("@idImagen", idImagen);
                    marcar.Parameters.AddWithValue("@idInmueble", idInmueble);
                    int filas = marcar.ExecuteNonQuery();

                    transaction.Commit();
                    return filas > 0;
                }
            }
        }

        private static ImagenInmueble Mapear(MySqlDataReader reader)
        {
            return new ImagenInmueble
            {
                IdImagen = reader.GetInt32("IdImagen"),
                IdInmueble = reader.GetInt32("IdInmueble"),
                RutaUrl = reader.GetString("RutaUrl"),
                EsPortada = reader.GetBoolean("EsPortada")
            };
        }
    }
}
