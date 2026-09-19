using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using InmobiliariaGrupoNN.Models;

namespace InmobiliariaGrupoNN.Repositories
{
    public class RepositorioReserva : IRepositorioReserva
    {
        private readonly string _connectionString;
        private readonly IRepositorioPago _repoPago;

        public RepositorioReserva(IConfiguration configuration, IRepositorioPago repoPago)
        {
            _repoPago = repoPago;
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentException("No se encontro la cadena de conexión en el appsettings.");
        }

        public IList<Reserva> ObtenerTodos(int numeroPagina = 1, int tamanio = 10)
        {
            if (numeroPagina < 1) numeroPagina = 1;
            if (tamanio < 1) tamanio = 10;

            long offset = ((long)numeroPagina - 1) * tamanio;
            var reservas = new List<Reserva>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string sql = @"
                        SELECT r.Id, r.InmuebleId, r.InquilinoId, r.FechaInicio, r.FechaFin, r.MontoPorDia,
                               i.Direccion,
                               inq.Nombre AS NombreInquilino, inq.Apellido AS ApellidoInquilino, r.EstadoActivo,
                               r.FechaFinalizacion, r.CreadoPorId, r.FinalizadoPorId, r.AnuladoPorId
                        FROM Reserva r
                        INNER JOIN Inmueble i ON r.InmuebleId = i.Id
                        INNER JOIN Inquilino inq ON r.InquilinoId = inq.Id
                        WHERE r.EstadoActivo = TRUE
                        ORDER BY r.Id DESC
                        LIMIT @tamanio OFFSET @offset";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@tamanio", tamanio);
                        command.Parameters.AddWithValue("@offset", offset);
                        
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reservas.Add(new Reserva
                                {
                                    Id = reader.GetInt32(0),
                                    InmuebleId = reader.GetInt32(1),
                                    InquilinoId = reader.GetInt32(2),
                                    FechaInicio = reader.GetDateTime(3),
                                    FechaFin = reader.GetDateTime(4),
                                    MontoPorDia = reader.GetDecimal(5),
                                    EstadoActivo = reader.GetBoolean(9),
                                    FechaFinalizacion = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                                    CreadoPorId = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                                    FinalizadoPorId = reader.IsDBNull(12) ? null : reader.GetInt32(12),
                                    AnuladoPorId = reader.IsDBNull(13) ? null : reader.GetInt32(13),
                                    Inmueble = new Inmueble { Direccion = reader.GetString(6) },
                                    Inquilino = new Inquilino 
                                    { 
                                        Nombre = reader.GetString(7), 
                                        Apellido = reader.GetString(8) 
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al obtener las reservas de la base de datos.", ex);
            }

            return reservas;
        }

        public int ObtenerCantidad()
        {
            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand("SELECT COUNT(*) FROM Reserva WHERE EstadoActivo = TRUE", connection);
            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar());
        }
        public Reserva? ObtenerPorId(int id)
        {
            if (id <= 0) throw new ArgumentException("El ID debe ser mayor a cero.");

            Reserva? reserva = null;

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string sql = @"
                        SELECT r.Id, r.InmuebleId, r.InquilinoId, r.FechaInicio, r.FechaFin, r.MontoPorDia,
                               i.Direccion,
                               inq.Nombre AS NombreInquilino, inq.Apellido AS ApellidoInquilino, r.EstadoActivo,
                               r.FechaFinalizacion, r.CreadoPorId, r.FinalizadoPorId, r.AnuladoPorId
                        FROM Reserva r
                        INNER JOIN Inmueble i ON r.InmuebleId = i.Id
                        INNER JOIN Inquilino inq ON r.InquilinoId = inq.Id
                        WHERE r.Id = @id";

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
                                    Id = reader.GetInt32(0),
                                    InmuebleId = reader.GetInt32(1),
                                    InquilinoId = reader.GetInt32(2),
                                    FechaInicio = reader.GetDateTime(3),
                                    FechaFin = reader.GetDateTime(4),
                                    MontoPorDia = reader.GetDecimal(5),
                                    EstadoActivo = reader.GetBoolean(9),
                                    FechaFinalizacion = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                                    CreadoPorId = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                                    FinalizadoPorId = reader.IsDBNull(12) ? null : reader.GetInt32(12),
                                    AnuladoPorId = reader.IsDBNull(13) ? null : reader.GetInt32(13),
                                    Inmueble = new Inmueble { Direccion = reader.GetString(6) },
                                    Inquilino = new Inquilino 
                                    { 
                                        Nombre = reader.GetString(7), 
                                        Apellido = reader.GetString(8) 
                                    }
                                };
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Error al buscar la reserva con ID {id}.", ex);
            }

            return reserva;
        }

        public int Alta(Reserva reserva)
        {
            ArgumentNullException.ThrowIfNull(reserva);
            reserva.Id = 0;
            reserva.EstadoActivo = true;
            reserva.FechaFinalizacion = null;
            reserva.FinalizadoPorId = null;
            reserva.AnuladoPorId = null;
            if (reserva.CreadoPorId.GetValueOrDefault() <= 0)
                throw new ArgumentException("No se pudo identificar al usuario creador.");

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            try
            {
                decimal porcentaje = BloquearInmueble(reserva.InmuebleId, connection, transaction, true);
                ValidarReserva(reserva);
                ValidarDisponibilidad(reserva, connection, transaction);
                decimal sena = CalculosReserva.CalcularSena(reserva, porcentaje);
                using var command = new MySqlCommand(@"INSERT INTO Reserva
                    (InmuebleId, InquilinoId, FechaInicio, FechaFin, MontoPorDia, CreadoPorId, EstadoActivo)
                    VALUES (@InmuebleId, @InquilinoId, @FechaInicio, @FechaFin, @MontoPorDia, @CreadoPorId, TRUE);
                    SELECT LAST_INSERT_ID();", connection, transaction);
                AgregarParametrosReserva(command, reserva);
                command.Parameters.AddWithValue("@CreadoPorId", reserva.CreadoPorId);
                int id = Convert.ToInt32(command.ExecuteScalar());
                if (id <= 0) throw new InvalidOperationException("No se pudo crear la reserva.");
                if (porcentaje > 0m)
                {
                    int pagoId = _repoPago.Alta(new Pago
                    {
                        ReservaId = id, Concepto = "Seña de reserva", FechaPago = DateTime.Today,
                        Importe = sena, CreadoPorId = reserva.CreadoPorId, EstadoActivo = true
                    }, connection, transaction);
                    if (pagoId <= 0) throw new InvalidOperationException("No se pudo registrar la seña.");
                }
                transaction.Commit();
                reserva.Id = id;
                return id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public int Modificacion(Reserva reserva)
        {
            if (reserva.Id <= 0) throw new ArgumentException("ID de reserva inválido.");
            ValidarReserva(reserva);
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            try
            {
                // Bloquear el destino antes de consultar disponibilidad, igual que en Alta.
                BloquearInmueble(reserva.InmuebleId, connection, transaction, false);
                var actual = BloquearReserva(reserva.Id, connection, transaction);
                // Comparar con la reserva bloqueada; el destino permanece bloqueado desde antes.
                if (actual.InmuebleId != reserva.InmuebleId)
                    BloquearInmueble(reserva.InmuebleId, connection, transaction, true);
                if (!actual.EstadoActivo || actual.FechaFinalizacion.HasValue)
                    throw new InvalidOperationException("No puede editarse una reserva anulada o finalizada anticipadamente.");
                ValidarDisponibilidad(reserva, connection, transaction);
                using var command = new MySqlCommand(@"UPDATE Reserva
                    SET InmuebleId = @InmuebleId, InquilinoId = @InquilinoId,
                        FechaInicio = @FechaInicio, FechaFin = @FechaFin, MontoPorDia = @MontoPorDia
                    WHERE Id = @Id AND EstadoActivo = TRUE AND FechaFinalizacion IS NULL", connection, transaction);
                AgregarParametrosReserva(command, reserva);
                command.Parameters.AddWithValue("@Id", reserva.Id);
                int filas = command.ExecuteNonQuery();
                // Cero también puede representar una edición sin cambios; la fila sigue bloqueada.
                if (filas < 0 || filas > 1) throw new InvalidOperationException("No se pudo modificar la reserva.");
                transaction.Commit();
                return filas;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public int Finalizar(int id, DateTime fechaFinalizacion, int usuarioId)
        {
            if (id <= 0 || usuarioId <= 0) throw new ArgumentException("Reserva o usuario no válido.");
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            try
            {
                var reserva = BloquearReserva(id, connection, transaction);
                var calculo = CalculosReserva.CalcularFinalizacion(reserva, fechaFinalizacion, DateTime.Today);
                int pagoId = _repoPago.Alta(new Pago
                {
                    ReservaId = id, Concepto = "Multa por finalización anticipada",
                    FechaPago = DateTime.Today, Importe = calculo.ImporteMulta,
                    CreadoPorId = usuarioId, EstadoActivo = true
                }, connection, transaction);
                if (pagoId <= 0) throw new InvalidOperationException("No se pudo registrar la multa.");
                using var command = new MySqlCommand(@"UPDATE Reserva
                    SET FechaFinalizacion = @Fecha, FinalizadoPorId = @UsuarioId
                    WHERE Id = @Id AND EstadoActivo = TRUE AND FechaFinalizacion IS NULL", connection, transaction);
                command.Parameters.AddWithValue("@Fecha", calculo.FechaFinalizacion);
                command.Parameters.AddWithValue("@UsuarioId", usuarioId);
                command.Parameters.AddWithValue("@Id", id);
                if (command.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException("No se pudo registrar la finalización.");
                transaction.Commit();
                return pagoId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static void AgregarParametrosReserva(MySqlCommand command, Reserva reserva)
        {
            command.Parameters.AddWithValue("@InmuebleId", reserva.InmuebleId);
            command.Parameters.AddWithValue("@InquilinoId", reserva.InquilinoId);
            command.Parameters.AddWithValue("@FechaInicio", reserva.FechaInicio);
            command.Parameters.AddWithValue("@FechaFin", reserva.FechaFin);
            command.Parameters.AddWithValue("@MontoPorDia", reserva.MontoPorDia);
        }

        private static Reserva BloquearReserva(int id, MySqlConnection connection, MySqlTransaction transaction)
        {
            using var command = new MySqlCommand(@"SELECT Id, InmuebleId, InquilinoId, FechaInicio, FechaFin,
                MontoPorDia, EstadoActivo, FechaFinalizacion FROM Reserva WHERE Id = @Id FOR UPDATE", connection, transaction);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();
            if (!reader.Read()) throw new InvalidOperationException("La reserva no existe.");
            return new Reserva
            {
                Id = reader.GetInt32("Id"), InmuebleId = reader.GetInt32("InmuebleId"),
                InquilinoId = reader.GetInt32("InquilinoId"), FechaInicio = reader.GetDateTime("FechaInicio"),
                FechaFin = reader.GetDateTime("FechaFin"), MontoPorDia = reader.GetDecimal("MontoPorDia"),
                EstadoActivo = reader.GetBoolean("EstadoActivo"),
                FechaFinalizacion = reader.IsDBNull(reader.GetOrdinal("FechaFinalizacion")) ? null : reader.GetDateTime("FechaFinalizacion")
            };
        }

        public int Baja(int id, int? anuladoPorId)
        {
            if (id <= 0) throw new ArgumentException("El ID proporcionado no es valido.");

            int res = -1;
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"UPDATE Reserva 
                                SET EstadoActivo = 0, AnuladoPorId = @AnuladoPorId 
                                WHERE Id = @Id AND EstadoActivo = TRUE;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    command.Parameters.AddWithValue("@AnuladoPorId", anuladoPorId.HasValue ? anuladoPorId.Value : DBNull.Value);
                    
                    connection.Open();
                    
                    try
                    {
                        res = command.ExecuteNonQuery();
                    }
                    catch (MySqlException ex)
                    {
                        throw new Exception("Error al intentar eliminar la reserva.", ex);
                    }
                }
            }
            return res;
        }

        // Metodo auxiliar para no repetir codigo
        private void ValidarReserva(Reserva reserva)
        {
            if (reserva == null) throw new ArgumentException("La reserva no puede ser nula.");
            reserva.FechaInicio = reserva.FechaInicio.Date;
            reserva.FechaFin = reserva.FechaFin.Date;
            if (reserva.FechaInicio < new DateTime(1000, 1, 1) || reserva.FechaFin < new DateTime(1000, 1, 1))
                throw new ArgumentException("Las fechas son obligatorias y deben ser válidas para MySQL.");
            if (reserva.InmuebleId <= 0) throw new ArgumentException("Debe seleccionar un Inmueble valido.");
            if (reserva.InquilinoId <= 0) throw new ArgumentException("Debe seleccionar un Inquilino valido.");
            CalculosReserva.ValidarMontoPorDia(reserva.MontoPorDia);
            
            if (reserva.FechaFin <= reserva.FechaInicio) 
            {
                throw new ArgumentException("La fecha de finalización debe ser posterior a la fecha de inicio.");
            }
            if (reserva.Id == 0 && reserva.FechaInicio.Date < DateTime.Today)
            {
                throw new ArgumentException("No se pueden crear reservas con fechas de inicio en el pasado.");
            }
        }

        private static decimal BloquearInmueble(int inmuebleId, MySqlConnection connection,
            MySqlTransaction transaction, bool exigirHabilitado)
        {
            using var command = new MySqlCommand(@"SELECT EstadoActivo, Disponible, PorcentajeReserva
                FROM Inmueble WHERE Id = @Id FOR UPDATE", connection, transaction);
            command.Parameters.AddWithValue("@Id", inmuebleId);
            using var reader = command.ExecuteReader();
            if (!reader.Read()) throw new ArgumentException("El inmueble seleccionado no existe.");
            if (exigirHabilitado && (!reader.GetBoolean("EstadoActivo") || !reader.GetBoolean("Disponible")))
                throw new InvalidOperationException("El inmueble ya no está habilitado para nuevas reservas.");
            return reader.GetDecimal("PorcentajeReserva");
        }

        private static void ValidarDisponibilidad(Reserva reserva, MySqlConnection connection, MySqlTransaction transaction)
        {
            using var command = new MySqlCommand(@"SELECT COUNT(*) FROM Reserva r
                WHERE r.InmuebleId = @InmuebleId AND r.EstadoActivo = TRUE
                  AND r.FechaInicio < @FechaFin
                  AND COALESCE(r.FechaFinalizacion, r.FechaFin) > @FechaInicio
                  AND COALESCE(r.FechaFinalizacion, r.FechaFin) > r.FechaInicio
                  AND r.Id != @Id", connection, transaction);
            command.Parameters.AddWithValue("@InmuebleId", reserva.InmuebleId);
            command.Parameters.AddWithValue("@FechaInicio", reserva.FechaInicio);
            command.Parameters.AddWithValue("@FechaFin", reserva.FechaFin);
            command.Parameters.AddWithValue("@Id", reserva.Id);
            if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                throw new InvalidOperationException("El inmueble ya se encuentra reservado en el rango de fechas seleccionado.");
        }
    }
}
