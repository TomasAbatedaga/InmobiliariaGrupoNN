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

            int offset = (numeroPagina - 1) * tamanio;
            var reservas = new List<Reserva>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string sql = @"
                        SELECT r.Id, r.InmuebleId, r.InquilinoId, r.FechaInicio, r.FechaFin, r.Monto,
                               i.Direccion,
                               inq.Nombre AS NombreInquilino, inq.Apellido AS ApellidoInquilino
                        FROM Reserva r
                        INNER JOIN Inmueble i ON r.InmuebleId = i.Id
                        INNER JOIN Inquilino inq ON r.InquilinoId = inq.Id
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
                                    Monto = reader.GetDecimal(5),
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

        public Reserva? ObtenerPorId(int id)
        {
            if (id <= 0) throw new ArgumentException("El ID debe ser mayor a cero.");

            Reserva? reserva = null;

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string sql = @"
                        SELECT r.Id, r.InmuebleId, r.InquilinoId, r.FechaInicio, r.FechaFin, r.Monto,
                               i.Direccion,
                               inq.Nombre AS NombreInquilino, inq.Apellido AS ApellidoInquilino
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
                                    Monto = reader.GetDecimal(5),
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
            ValidarReserva(reserva);

            int res = -1;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string sql = @"INSERT INTO Reserva (InmuebleId, InquilinoId, FechaInicio, FechaFin, Monto) 
                                   VALUES (@inmuebleId, @inquilinoId, @fechaInicio, @fechaFin, @monto); 
                                   SELECT LAST_INSERT_ID();";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@inmuebleId", reserva.InmuebleId);
                        command.Parameters.AddWithValue("@inquilinoId", reserva.InquilinoId);
                        command.Parameters.AddWithValue("@fechaInicio", reserva.FechaInicio);
                        command.Parameters.AddWithValue("@fechaFin", reserva.FechaFin);
                        command.Parameters.AddWithValue("@monto", reserva.Monto);
                        
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

            int res = -1;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string sql = @"UPDATE Reserva 
                                   SET InmuebleId = @inmuebleId, InquilinoId = @inquilinoId, 
                                       FechaInicio = @fechaInicio, FechaFin = @fechaFin, Monto = @monto 
                                   WHERE Id = @id";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@inmuebleId", reserva.InmuebleId);
                        command.Parameters.AddWithValue("@inquilinoId", reserva.InquilinoId);
                        command.Parameters.AddWithValue("@fechaInicio", reserva.FechaInicio);
                        command.Parameters.AddWithValue("@fechaFin", reserva.FechaFin);
                        command.Parameters.AddWithValue("@monto", reserva.Monto);
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

        public int Baja(int id)
        {
            if (id <= 0) throw new ArgumentException("El ID proporcionado no es válido.");

            int res = -1;
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "DELETE FROM Reserva WHERE Id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
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
            if (reserva.InmuebleId <= 0) throw new ArgumentException("Debe seleccionar un Inmueble valido.");
            if (reserva.InquilinoId <= 0) throw new ArgumentException("Debe seleccionar un Inquilino valido.");
            if (reserva.Monto <= 0) throw new ArgumentException("El monto de la reserva debe ser mayor a cero.");
            if (reserva.FechaFin <= reserva.FechaInicio) 
            {
                throw new ArgumentException("La fecha de finalizacion debe ser posterior a la fecha de inicio.");
            }
        }
    }
}