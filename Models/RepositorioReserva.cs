using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioReserva : RepositorioBase, IRepositorioReserva
    {
        // Constructor corregido para recibir IConfiguration y pasarlo a base()
        public RepositorioReserva(IConfiguration configuration) : base(configuration) 
        { 
        }
        public List<Reserva> ObtenerTodos()
        {
            var lista = new List<Reserva>();
            // Usamos ObtenerConexion() en lugar de new MySqlConnection(ConnectionString)
            using (var connection = ObtenerConexion())
            {
                var sql = @"
                    SELECT r.*, 
                           inq.NombreCompleto AS InquilinoNombre, 
                           inm.Direccion AS InmuebleDireccion,
                           uc.NombreCompleto AS UsuarioCreacionNombre
                    FROM Reserva r
                    INNER JOIN Inquilino inq ON r.IdInquilino = inq.IdInquilino
                    INNER JOIN Inmueble inm ON r.IdInmueble = inm.IdInmueble
                    INNER JOIN Usuario uc ON r.IdUsuarioCreacion = uc.IdUsuario
                    ORDER BY r.IdReserva DESC;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Reserva
                            {
                                IdReserva = reader.GetInt32("IdReserva"),
                                IdInquilino = reader.GetInt32("IdInquilino"),
                                IdInmueble = reader.GetInt32("IdInmueble"),
                                IdUsuarioCreacion = reader.GetInt32("IdUsuarioCreacion"),
                                FechaDesde = reader.GetDateTime("FechaDesde"),
                                FechaHasta = reader.GetDateTime("FechaHasta"),
                                MontoDiario = reader.GetDecimal("MontoDiario"),
                                FechaFinalizacion = reader.IsDBNull(reader.GetOrdinal("FechaFinalizacion")) ? null : reader.GetDateTime("FechaFinalizacion"),
                                Multa = reader.IsDBNull(reader.GetOrdinal("Multa")) ? null : reader.GetDecimal("Multa"),
                                IdUsuarioFinalizacion = reader.IsDBNull(reader.GetOrdinal("IdUsuarioFinalizacion")) ? null : reader.GetInt32("IdUsuarioFinalizacion"),
                                Inquilino = new Inquilino { NombreCompleto = reader.GetString("InquilinoNombre") },
                                Inmueble = new Inmueble { Direccion = reader.GetString("InmuebleDireccion") },
                                UsuarioCreacion = new Usuario { NombreCompleto = reader.GetString("UsuarioCreacionNombre") }
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public Reserva? ObtenerPorId(int id)
        {
            Reserva? reserva = null;
            using (var connection = ObtenerConexion())
            {
                var sql = @"
                    SELECT r.*, 
                           inq.NombreCompleto AS InquilinoNombre, 
                           inm.Direccion AS InmuebleDireccion
                    FROM Reserva r
                    INNER JOIN Inquilino inq ON r.IdInquilino = inq.IdInquilino
                    INNER JOIN Inmueble inm ON r.IdInmueble = inm.IdInmueble
                    WHERE r.IdReserva = @id;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            reserva = new Reserva
                            {
                                IdReserva = reader.GetInt32("IdReserva"),
                                IdInquilino = reader.GetInt32("IdInquilino"),
                                IdInmueble = reader.GetInt32("IdInmueble"),
                                IdUsuarioCreacion = reader.GetInt32("IdUsuarioCreacion"),
                                FechaDesde = reader.GetDateTime("FechaDesde"),
                                FechaHasta = reader.GetDateTime("FechaHasta"),
                                MontoDiario = reader.GetDecimal("MontoDiario"),
                                FechaFinalizacion = reader.IsDBNull(reader.GetOrdinal("FechaFinalizacion")) ? null : reader.GetDateTime("FechaFinalizacion"),
                                Multa = reader.IsDBNull(reader.GetOrdinal("Multa")) ? null : reader.GetDecimal("Multa"),
                                IdUsuarioFinalizacion = reader.IsDBNull(reader.GetOrdinal("IdUsuarioFinalizacion")) ? null : reader.GetInt32("IdUsuarioFinalizacion"),
                                Inquilino = new Inquilino { NombreCompleto = reader.GetString("InquilinoNombre") },
                                Inmueble = new Inmueble { Direccion = reader.GetString("InmuebleDireccion") }
                            };
                        }
                    }
                }
            }
            return reserva;
        }

        public int Alta(Reserva reserva)
{
    using (var connection = ObtenerConexion())
    {
        // 1. PRIMERO: VALIDAR QUE NO HAYA SUPERPOSICIÓN DE FECHAS
        var sqlValidacion = @"
            SELECT COUNT(*) FROM Reserva 
            WHERE IdInmueble = @IdInmueble 
            AND FechaDesde <= @FechaHasta 
            AND FechaHasta >= @FechaDesde;";

        using (var commandValidacion = new MySqlCommand(sqlValidacion, connection))
        {
            commandValidacion.Parameters.AddWithValue("@IdInmueble", reserva.IdInmueble);
            commandValidacion.Parameters.AddWithValue("@FechaDesde", reserva.FechaDesde);
            commandValidacion.Parameters.AddWithValue("@FechaHasta", reserva.FechaHasta);

            connection.Open();
            int reservasExistentes = Convert.ToInt32(commandValidacion.ExecuteScalar());

            // Si el conteo es mayor a 0, significa que ya hay una reserva en esas fechas
            if (reservasExistentes > 0)
            {
                throw new Exception("El inmueble ya se encuentra reservado en las fechas seleccionadas. Por favor, elija otras fechas.");
            }
        }

        // 2. SEGUNDO: SI PASA LA VALIDACIÓN, RECién ahí hacemos el INSERT
        var sqlInsert = @"
            INSERT INTO Reserva 
            (IdInquilino, IdInmueble, IdUsuarioCreacion, FechaDesde, FechaHasta, MontoDiario)
            VALUES 
            (@IdInquilino, @IdInmueble, @IdUsuarioCreacion, @FechaDesde, @FechaHasta, @MontoDiario);
            SELECT LAST_INSERT_ID();";

        using (var commandInsert = new MySqlCommand(sqlInsert, connection))
        {
            commandInsert.Parameters.AddWithValue("@IdInquilino", reserva.IdInquilino);
            commandInsert.Parameters.AddWithValue("@IdInmueble", reserva.IdInmueble);
            commandInsert.Parameters.AddWithValue("@IdUsuarioCreacion", reserva.IdUsuarioCreacion);
            commandInsert.Parameters.AddWithValue("@FechaDesde", reserva.FechaDesde);
            commandInsert.Parameters.AddWithValue("@FechaHasta", reserva.FechaHasta);
            commandInsert.Parameters.AddWithValue("@MontoDiario", reserva.MontoDiario);

            return Convert.ToInt32(commandInsert.ExecuteScalar());
        }
    }
}

        public int Modificacion(Reserva reserva)
        {
            using (var connection = ObtenerConexion())
            {
                var sql = @"
                    UPDATE Reserva 
                    SET IdInquilino = @IdInquilino,
                        IdInmueble = @IdInmueble,
                        FechaDesde = @FechaDesde,
                        FechaHasta = @FechaHasta,
                        MontoDiario = @MontoDiario
                    WHERE IdReserva = @IdReserva;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@IdReserva", reserva.IdReserva);
                    command.Parameters.AddWithValue("@IdInquilino", reserva.IdInquilino);
                    command.Parameters.AddWithValue("@IdInmueble", reserva.IdInmueble);
                    command.Parameters.AddWithValue("@FechaDesde", reserva.FechaDesde);
                    command.Parameters.AddWithValue("@FechaHasta", reserva.FechaHasta);
                    command.Parameters.AddWithValue("@MontoDiario", reserva.MontoDiario);

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int FinalizarAnticipadamente(int idReserva, DateTime fechaFinalizacion, decimal multa, int idUsuarioFinalizacion)
        {
            using (var connection = ObtenerConexion())
            {
                var sql = @"
                    UPDATE Reserva 
                    SET FechaFinalizacion = @FechaFinalizacion,
                        Multa = @Multa,
                        IdUsuarioFinalizacion = @IdUsuarioFinalizacion
                    WHERE IdReserva = @IdReserva;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@IdReserva", idReserva);
                    command.Parameters.AddWithValue("@FechaFinalizacion", fechaFinalizacion);
                    command.Parameters.AddWithValue("@Multa", multa);
                    command.Parameters.AddWithValue("@IdUsuarioFinalizacion", idUsuarioFinalizacion);

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int Eliminar(int id)
        {
            using (var connection = ObtenerConexion())
            {
                var sql = "DELETE FROM Reserva WHERE IdReserva = @id;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }
    }
}