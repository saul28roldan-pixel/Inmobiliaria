using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioTipoInmueble : RepositorioBase, IRepositorioTipoInmueble
    {
        public int Alta(TipoInmueble t)
        {
            int id = 0;
            string sql = @"INSERT INTO TipoInmueble (Descripcion)
                            VALUES (@descripcion);
                            SELECT LAST_INSERT_ID();";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@descripcion", t.Descripcion);

                connection.Open();
                id = Convert.ToInt32(command.ExecuteScalar());
            }
            t.IdTipo = id;
            return id;
        }

        public bool Baja(int id)
        {
            int filasAfectadas = 0;
            string sql = "DELETE FROM TipoInmueble WHERE IdTipo = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }
            return filasAfectadas > 0;
        }

        public bool Modificacion(TipoInmueble t)
        {
            int filasAfectadas = 0;
            string sql = @"UPDATE TipoInmueble
                            SET Descripcion = @descripcion
                            WHERE IdTipo = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@descripcion", t.Descripcion);
                command.Parameters.AddWithValue("@id", t.IdTipo);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }
            return filasAfectadas > 0;
        }

        public IList<TipoInmueble> ObtenerTodos()
        {
            var lista = new List<TipoInmueble>();
            string sql = @"SELECT IdTipo, Descripcion
                            FROM TipoInmueble
                            ORDER BY Descripcion";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(MapearTipoInmueble(reader));
                    }
                }
            }
            return lista;
        }

        public TipoInmueble? ObtenerPorId(int id)
        {
            TipoInmueble? t = null;
            string sql = @"SELECT IdTipo, Descripcion
                            FROM TipoInmueble
                            WHERE IdTipo = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        t = MapearTipoInmueble(reader);
                    }
                }
            }
            return t;
        }

        // Convierte la fila actual del reader en un objeto TipoInmueble.
        private static TipoInmueble MapearTipoInmueble(MySqlDataReader reader)
        {
            return new TipoInmueble
            {
                IdTipo = reader.GetInt32("IdTipo"),
                Descripcion = reader.GetString("Descripcion"),
            };
        }
    }
}