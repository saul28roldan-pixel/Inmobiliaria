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
        public IList<Propietario> ObtenerFiltradosPaginados(
    string? busqueda,
    int pagina,
    int registrosPorPagina,
    out int totalRegistros)
{
    totalRegistros = 0;

    var lista = new List<Propietario>();

    string sqlBase = @"FROM Propietario";

    using (var connection = ObtenerConexion())
    {
        connection.Open();

        // ==========================================
        // 1. CONTAR REGISTROS
        // ==========================================

        string sqlCount = "SELECT COUNT(*) " + sqlBase;

        var cmdCount = new MySqlCommand();
        cmdCount.Connection = connection;

        string sqlWhere = "";

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            sqlWhere = @"
                WHERE Nombre LIKE @busqueda
                   OR Apellido LIKE @busqueda";

            cmdCount.Parameters.AddWithValue(
                "@busqueda",
                $"%{busqueda}%"
            );
        }

        cmdCount.CommandText =
            sqlCount + sqlWhere;

        totalRegistros =
            Convert.ToInt32(
                cmdCount.ExecuteScalar()
            );


        // ==========================================
        // 2. OBTENER LOS DATOS PAGINADOS
        // ==========================================

        string sqlData = @"
            SELECT IdPropietario,
                   Nombre,
                   Apellido,
                   Dni,
                   Email,
                   Telefono
            " + sqlBase + sqlWhere + @"
            ORDER BY Apellido, Nombre
            LIMIT @limit OFFSET @offset";

        var cmdData = new MySqlCommand(
            sqlData,
            connection
        );


        // Copiar parámetros del COUNT

        foreach (MySqlParameter parametro
                 in cmdCount.Parameters)
        {
            cmdData.Parameters.AddWithValue(
                parametro.ParameterName,
                parametro.Value
            );
        }


        int offset =
            (pagina - 1) * registrosPorPagina;


        cmdData.Parameters.AddWithValue(
            "@limit",
            registrosPorPagina
        );

        cmdData.Parameters.AddWithValue(
            "@offset",
            offset
        );


        using (var reader = cmdData.ExecuteReader())
        {
            while (reader.Read())
            {
                lista.Add(
                    MapearPropietario(reader)
                );
            }
        }
    }

    return lista;
}
    
    public List<object> BuscarParaSelect(string termino, int maxResultados = 10)
{
    var resultados = new List<object>();

    if (string.IsNullOrWhiteSpace(termino))
        return resultados;

    string sql = @"
        SELECT IdPropietario AS id, 
               CONCAT(Nombre, ' ', Apellido, ' - DNI: ', Dni) AS text
        FROM Propietario
        WHERE Nombre LIKE @q OR Apellido LIKE @q OR Dni LIKE @q
        ORDER BY Apellido, Nombre
        LIMIT @limit";

    using (var connection = ObtenerConexion())
    {
        var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@q", $"%{termino}%");
        command.Parameters.AddWithValue("@limit", maxResultados);

        connection.Open();
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                // Devolvemos un tipo anónimo con 'id' y 'text', 
                // que es exactamente el formato que espera Select2
                resultados.Add(new
                {
                    id = reader.GetInt32("id"),
                    text = reader.GetString("text")
                });
            }
        }
    }
    return resultados;
}
}
}