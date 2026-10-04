using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public class RepositorioTipoInmueble : RepositorioBase, IRepositorioTipoInmueble
    {
        public RepositorioTipoInmueble(IConfiguration configuration) : base(configuration) 
        { 
        }

        public int Alta(TipoInmueble t)
        {
            int id = 0;
            string sql = @"INSERT INTO TipoInmueble (Descripcion) VALUES (@descripcion);
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
            string sql = "UPDATE TipoInmueble SET Descripcion = @descripcion WHERE IdTipo = @id";

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
            string sql = "SELECT IdTipo, Descripcion FROM TipoInmueble ORDER BY Descripcion";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new TipoInmueble
                        {
                            IdTipo = reader.GetInt32("IdTipo"),
                            Descripcion = reader.GetString("Descripcion")
                        });
                    }
                }
            }
            return lista;
        }

        public TipoInmueble? ObtenerPorId(int id)
        {
            TipoInmueble? t = null;
            string sql = "SELECT IdTipo, Descripcion FROM TipoInmueble WHERE IdTipo = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        t = new TipoInmueble
                        {
                            IdTipo = reader.GetInt32("IdTipo"),
                            Descripcion = reader.GetString("Descripcion")
                        };
                    }
                }
            }
            return t;
        }

        // NUEVO MÉTODO: Filtrado y paginado en la BD
        public IList<TipoInmueble> ObtenerFiltradosPaginados(string? busqueda, int pagina, int registrosPorPagina, out int totalRegistros)
        {
            totalRegistros = 0;
            var lista = new List<TipoInmueble>();

            using (var connection = ObtenerConexion())
            {
                connection.Open();

                // 1. Contar total
                string sqlCount = "SELECT COUNT(*) FROM TipoInmueble";
                var cmdCount = new MySqlCommand { Connection = connection };
                string sqlWhere = "";

                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    sqlWhere = " WHERE Descripcion LIKE @busqueda";
                    cmdCount.Parameters.AddWithValue("@busqueda", $"%{busqueda}%");
                }

                cmdCount.CommandText = sqlCount + sqlWhere;
                totalRegistros = Convert.ToInt32(cmdCount.ExecuteScalar());

                // 2. Obtener datos paginados
                string sqlData = "SELECT IdTipo, Descripcion FROM TipoInmueble" + sqlWhere + 
                                 " ORDER BY Descripcion LIMIT @limit OFFSET @offset";

                var cmdData = new MySqlCommand(sqlData, connection);
                foreach (MySqlParameter param in cmdCount.Parameters)
                {
                    cmdData.Parameters.AddWithValue(param.ParameterName, param.Value);
                }

                int offset = (pagina - 1) * registrosPorPagina;
                cmdData.Parameters.AddWithValue("@limit", registrosPorPagina);
                cmdData.Parameters.AddWithValue("@offset", offset);

                using (var reader = cmdData.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new TipoInmueble
                        {
                            IdTipo = reader.GetInt32("IdTipo"),
                            Descripcion = reader.GetString("Descripcion")
                        });
                    }
                }
            }
            return lista;
        }
    }
}