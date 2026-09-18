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

        public RepositorioReserva(IConfiguration configuration)
        {
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
                               inq.Nombre AS NombreInquilino, inq.Apellido AS ApellidoInquilino, r.EstadoActivo
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
                               inq.Nombre AS NombreInquilino, inq.Apellido AS ApellidoInquilino, r.EstadoActivo
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
            ValidarReserva(reserva);
            ValidarInmuebleParaAlta(reserva.InmuebleId);
            ValidarDisponibilidad(reserva);

            int res = -1;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string sql = @"INSERT INTO Reserva 
                                    (InmuebleId, InquilinoId, FechaInicio, FechaFin, MontoPorDia, CreadoPorId, EstadoActivo)
                                    VALUES (@InmuebleId, @InquilinoId, @FechaInicio, @FechaFin, @MontoPorDia, @CreadoPorId, TRUE);
                                    SELECT LAST_INSERT_ID();";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@inmuebleId", reserva.InmuebleId);
                        command.Parameters.AddWithValue("@inquilinoId", reserva.InquilinoId);
                        command.Parameters.AddWithValue("@fechaInicio", reserva.FechaInicio);
                        command.Parameters.AddWithValue("@fechaFin", reserva.FechaFin);
                        command.Parameters.AddWithValue("@montoPorDia", reserva.MontoPorDia);
                        command.Parameters.AddWithValue("@CreadoPorId", reserva.CreadoPorId.HasValue ? reserva.CreadoPorId.Value : DBNull.Value);
                        
                        connection.Open();
                        res = Convert.ToInt32(command.ExecuteScalar());
                        reserva.Id = res;
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al intentar guardar la nueva reserva.", ex);
            }
            return res;
        }

        public int Modificacion(Reserva reserva)
        {
            if (reserva.Id <= 0) throw new ArgumentException("ID de reserva inválido para modificación.");
            ValidarReserva(reserva);
            ValidarDisponibilidad(reserva);

            int res = -1;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string sql = @"UPDATE Reserva 
                                   SET InmuebleId = @inmuebleId, InquilinoId = @inquilinoId, 
                                       FechaInicio = @fechaInicio, FechaFin = @fechaFin, MontoPorDia = @montoPorDia 
                                   WHERE Id = @id AND EstadoActivo = TRUE";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@inmuebleId", reserva.InmuebleId);
                        command.Parameters.AddWithValue("@inquilinoId", reserva.InquilinoId);
                        command.Parameters.AddWithValue("@fechaInicio", reserva.FechaInicio);
                        command.Parameters.AddWithValue("@fechaFin", reserva.FechaFin);
                        command.Parameters.AddWithValue("@montoPorDia", reserva.MontoPorDia);
                        command.Parameters.AddWithValue("@id", reserva.Id);
                        
                        connection.Open();
                        res = command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al intentar actualizar la reserva.", ex);
            }
            return res;
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
            if (reserva.MontoPorDia <= 0) throw new ArgumentException("El monto de la reserva debe ser mayor a cero.");
            
            if (reserva.FechaFin <= reserva.FechaInicio) 
            {
                throw new ArgumentException("La fecha de finalización debe ser posterior a la fecha de inicio.");
            }
            if (reserva.Id == 0 && reserva.FechaInicio.Date < DateTime.Today)
            {
                throw new ArgumentException("No se pueden crear reservas con fechas de inicio en el pasado.");
            }
        }

        private void ValidarInmuebleParaAlta(int inmuebleId)
        {
            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(
                "SELECT EstadoActivo, Disponible FROM Inmueble WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", inmuebleId);
            connection.Open();
            using var reader = command.ExecuteReader();
            if (!reader.Read()) throw new ArgumentException("El inmueble seleccionado no existe.");
            if (!reader.GetBoolean("EstadoActivo") || !reader.GetBoolean("Disponible"))
                throw new InvalidOperationException("El inmueble ya no está habilitado para nuevas reservas.");
        }
        private void ValidarDisponibilidad(Reserva reserva)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"SELECT COUNT(*) FROM Reserva 
                               WHERE InmuebleId = @inmuebleId
                               AND EstadoActivo = TRUE
                               AND FechaInicio < @fechaFin 
                               AND FechaFin > @fechaInicio
                               AND Id != @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@inmuebleId", reserva.InmuebleId);
                    command.Parameters.AddWithValue("@fechaInicio", reserva.FechaInicio);
                    command.Parameters.AddWithValue("@fechaFin", reserva.FechaFin);
                    command.Parameters.AddWithValue("@id", reserva.Id);

                    connection.Open();
                    int reservasSuperpuestas = Convert.ToInt32(command.ExecuteScalar());

                    if (reservasSuperpuestas > 0)
                    {
                        throw new InvalidOperationException("El inmueble ya se encuentra reservado en el rango de fechas seleccionado.");
                    }
                }
            }
        }

    }
}