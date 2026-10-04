using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioReserva : RepositorioBase, IRepositorioReserva
    {
        public RepositorioReserva(IConfiguration configuration) : base(configuration) 
        { 
        }

        public List<Reserva> ObtenerTodos()
        {
            var lista = new List<Reserva>();
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
                                FechaFinalizacion = reader.IsDBNull(reader.GetOrdinal("FechaFinalizacion")) ? (DateTime?)null : reader.GetDateTime("FechaFinalizacion"),
                                Multa = reader.IsDBNull(reader.GetOrdinal("Multa")) ? (decimal?)null : reader.GetDecimal("Multa"),
                                IdUsuarioFinalizacion = reader.IsDBNull(reader.GetOrdinal("IdUsuarioFinalizacion")) ? (int?)null : reader.GetInt32("IdUsuarioFinalizacion"),
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
                                FechaFinalizacion = reader.IsDBNull(reader.GetOrdinal("FechaFinalizacion")) ? (DateTime?)null : reader.GetDateTime("FechaFinalizacion"),
                                Multa = reader.IsDBNull(reader.GetOrdinal("Multa")) ? (decimal?)null : reader.GetDecimal("Multa"),
                                IdUsuarioFinalizacion = reader.IsDBNull(reader.GetOrdinal("IdUsuarioFinalizacion")) ? (int?)null : reader.GetInt32("IdUsuarioFinalizacion"),
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

                    if (reservasExistentes > 0)
                    {
                        throw new Exception("El inmueble ya se encuentra reservado en las fechas seleccionadas. Por favor, elija otras fechas.");
                    }
                }

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

        // ==========================================================
        // NUEVO MÉTODO: Filtrado y paginado directamente en la BD
        // ==========================================================
        public List<Reserva> ObtenerFiltradosPaginados(string? busqueda, int pagina, int registrosPorPagina, out int totalRegistros)
        {
            totalRegistros = 0;
            var lista = new List<Reserva>();
            var whereConditions = new List<string>();

            string sqlBase = @"FROM Reserva r
                               INNER JOIN Inquilino inq ON r.IdInquilino = inq.IdInquilino
                               INNER JOIN Inmueble inm ON r.IdInmueble = inm.IdInmueble
                               INNER JOIN Usuario uc ON r.IdUsuarioCreacion = uc.IdUsuario";

            using (var connection = ObtenerConexion())
            {
                connection.Open();

                // 1. OBTENER EL TOTAL DE REGISTROS
                string sqlCount = "SELECT COUNT(*) " + sqlBase;
                var cmdCount = new MySqlCommand { Connection = connection };

                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    whereConditions.Add("(inq.NombreCompleto LIKE @busqueda OR inm.Direccion LIKE @busqueda)");
                    cmdCount.Parameters.AddWithValue("@busqueda", $"%{busqueda}%");
                }

                string sqlWhere = whereConditions.Count > 0 ? " WHERE " + string.Join(" AND ", whereConditions) : "";
                cmdCount.CommandText = sqlCount + sqlWhere;
                totalRegistros = Convert.ToInt32(cmdCount.ExecuteScalar());

                // 2. OBTENER LOS DATOS PAGINADOS
                string sqlData = @"SELECT r.*, 
                                          inq.NombreCompleto AS InquilinoNombre, 
                                          inm.Direccion AS InmuebleDireccion,
                                          uc.NombreCompleto AS UsuarioCreacionNombre " 
                                  + sqlBase + sqlWhere + @"
                                   ORDER BY r.IdReserva DESC 
                                   LIMIT @limit OFFSET @offset";

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
                        lista.Add(new Reserva
                        {
                            IdReserva = reader.GetInt32("IdReserva"),
                            IdInquilino = reader.GetInt32("IdInquilino"),
                            IdInmueble = reader.GetInt32("IdInmueble"),
                            IdUsuarioCreacion = reader.GetInt32("IdUsuarioCreacion"),
                            FechaDesde = reader.GetDateTime("FechaDesde"),
                            FechaHasta = reader.GetDateTime("FechaHasta"),
                            MontoDiario = reader.GetDecimal("MontoDiario"),
                            FechaFinalizacion = reader.IsDBNull(reader.GetOrdinal("FechaFinalizacion")) ? (DateTime?)null : reader.GetDateTime("FechaFinalizacion"),
                            Multa = reader.IsDBNull(reader.GetOrdinal("Multa")) ? (decimal?)null : reader.GetDecimal("Multa"),
                            IdUsuarioFinalizacion = reader.IsDBNull(reader.GetOrdinal("IdUsuarioFinalizacion")) ? (int?)null : reader.GetInt32("IdUsuarioFinalizacion"),
                            Inquilino = new Inquilino { NombreCompleto = reader.GetString("InquilinoNombre") },
                            Inmueble = new Inmueble { Direccion = reader.GetString("InmuebleDireccion") },
                            UsuarioCreacion = new Usuario { NombreCompleto = reader.GetString("UsuarioCreacionNombre") }
                        });
                    }
                }
            }
            return lista;
        }
    }
}