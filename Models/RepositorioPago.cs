using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioPago : RepositorioBase, IRepositorioPago
    {
        public RepositorioPago(IConfiguration configuration) : base(configuration)
        {
        }

        // ==========================================================
        // ALTA: Crear un nuevo pago
        // ==========================================================
        public int Alta(Pago p)
        {
            int id = 0;
            string sql = @"INSERT INTO pago (IdReserva, IdUsuarioCreacion, Concepto, FechaPago, Importe, Anulado)
                           VALUES (@idReserva, @idUsuarioCreacion, @concepto, @fechaPago, @importe, 0);
                           SELECT LAST_INSERT_ID();";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@idReserva", p.IdReserva);
                command.Parameters.AddWithValue("@idUsuarioCreacion", p.IdUsuarioCreacion);
                command.Parameters.AddWithValue("@concepto", p.Concepto);
                command.Parameters.AddWithValue("@fechaPago", p.FechaPago);
                command.Parameters.AddWithValue("@importe", p.Importe);

                connection.Open();
                id = Convert.ToInt32(command.ExecuteScalar());
            }
            p.IdPago = id;
            return id;
        }

        // ==========================================================
        // ANULAR: Cambiar estado a anulado (no borra el registro)
        // ==========================================================
        public bool Anular(int idPago, int idUsuarioAnulacion)
        {
            int filasAfectadas = 0;
            string sql = @"UPDATE pago 
                           SET Anulado = 1, 
                               IdUsuarioAnulacion = @idUsuarioAnulacion, 
                               FechaAnulacion = @fechaAnulacion
                           WHERE IdPago = @idPago AND Anulado = 0";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@idPago", idPago);
                command.Parameters.AddWithValue("@idUsuarioAnulacion", idUsuarioAnulacion);
                command.Parameters.AddWithValue("@fechaAnulacion", DateTime.Now);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }
            return filasAfectadas > 0;
        }

        // ==========================================================
        // OBTENER POR ID
        // ==========================================================
        public Pago? ObtenerPorId(int id)
        {
            Pago? pago = null;
            string sql = @"SELECT IdPago, IdReserva, IdUsuarioCreacion, Concepto, 
                                  FechaPago, Importe, Anulado, IdUsuarioAnulacion, FechaAnulacion
                           FROM pago
                           WHERE IdPago = @id";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        pago = MapearPago(reader);
                    }
                }
            }
            return pago;
        }

        // ==========================================================
        // OBTENER TODOS LOS PAGOS DE UNA RESERVA
        // ==========================================================
        public IList<Pago> ObtenerPorReserva(int idReserva)
        {
            var lista = new List<Pago>();
            string sql = @"SELECT IdPago, IdReserva, IdUsuarioCreacion, Concepto, 
                                  FechaPago, Importe, Anulado, IdUsuarioAnulacion, FechaAnulacion
                           FROM pago
                           WHERE IdReserva = @idReserva
                           ORDER BY FechaPago DESC";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@idReserva", idReserva);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(MapearPago(reader));
                    }
                }
            }
            return lista;
        }

        // ==========================================================
        // MODIFICAR SOLO EL CONCEPTO (regla del profesor)
        // ==========================================================
        public bool ModificacionConcepto(int idPago, string nuevoConcepto)
        {
            int filasAfectadas = 0;
            string sql = @"UPDATE pago 
                           SET Concepto = @concepto 
                           WHERE IdPago = @idPago AND Anulado = 0";

            using (var connection = ObtenerConexion())
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@concepto", nuevoConcepto);
                command.Parameters.AddWithValue("@idPago", idPago);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }
            return filasAfectadas > 0;
        }

        // ==========================================================
        // MÉTODO PRIVADO: Mapear el DataReader al objeto Pago
        // ==========================================================
        private static Pago MapearPago(MySqlDataReader reader)
        {
            return new Pago
            {
                IdPago = reader.GetInt32("IdPago"),
                IdReserva = reader.GetInt32("IdReserva"),
                IdUsuarioCreacion = reader.GetInt32("IdUsuarioCreacion"),
                Concepto = reader.GetString("Concepto"),
                FechaPago = reader.GetDateTime("FechaPago"),
                Importe = reader.GetDecimal("Importe"),
                Anulado = reader.GetBoolean("Anulado"),
                IdUsuarioAnulacion = reader.IsDBNull(reader.GetOrdinal("IdUsuarioAnulacion")) 
                    ? null 
                    : reader.GetInt32("IdUsuarioAnulacion"),
                FechaAnulacion = reader.IsDBNull(reader.GetOrdinal("FechaAnulacion")) 
                    ? null 
                    : reader.GetDateTime("FechaAnulacion")
            };
        }
    }
}