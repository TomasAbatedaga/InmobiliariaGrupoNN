using InmobiliariaGrupoNN.Models;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace InmobiliariaGrupoNN.Repositories
{
    public class RepositorioPropietario : IRepositorioPropietario
    {
        private readonly string _connectionString;

        public RepositorioPropietario(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? String.Empty;
        }

        public IList<Propietario> ObtenerTodos()
        {
            var propietarios = new List<Propietario>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "SELECT Id, Dni, Nombre, Apellido, Telefono, Email, EstadoActivo, FechaAlta, FechaBaja FROM Propietario WHERE EstadoActivo = 1";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            propietarios.Add(new Propietario
                            {
                                Id = reader.GetInt32(0),
                                Dni = reader.GetString(1),
                                Nombre = reader.GetString(2),
                                Apellido = reader.GetString(3),
                                Telefono = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                                EstadoActivo = reader.GetBoolean(6),
                                FechaAlta = reader.GetDateTime(7),
                                FechaBaja = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
                            });
                        }
                    }
                }
            }
            return propietarios;
        }

        public Propietario? ObtenerPorId(int id)
        {
            Propietario? propietario = null;
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "SELECT Id, Dni, Nombre, Apellido, Telefono, Email, EstadoActivo, FechaAlta, FechaBaja FROM Propietario WHERE Id = @id";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            propietario = new Propietario
                            {
                                Id = reader.GetInt32(0),
                                Dni = reader.GetString(1),
                                Nombre = reader.GetString(2),
                                Apellido = reader.GetString(3),
                                Telefono = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                                EstadoActivo = reader.GetBoolean(6),
                                FechaAlta = reader.GetDateTime(7),
                                FechaBaja = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
                            };
                        }
                    }
                }
            }
            return propietario;
        }

        public int Alta(Propietario propietario)
        {
            int id = 0;
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Propietario (Dni, Nombre, Apellido, Telefono, Email) 
                               VALUES (@Dni, @Nombre, @Apellido, @Telefono, @Email);
                               SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Dni", propietario.Dni);
                    command.Parameters.AddWithValue("@Nombre", propietario.Nombre);
                    command.Parameters.AddWithValue("@Apellido", propietario.Apellido);
                    command.Parameters.AddWithValue("@Telefono", propietario.Telefono ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Email", propietario.Email ?? (object)DBNull.Value);

                    connection.Open();
                    id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return id;
        }

        public int Modificacion(Propietario propietario)
        {
            int filasAfectadas = 0;
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"UPDATE Propietario 
                               SET Dni = @Dni, 
                               Nombre = @Nombre, 
                               Apellido = @Apellido, 
                               Telefono = @Telefono, 
                               Email = @Email 
                               WHERE Id = @Id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Dni", propietario.Dni);
                    command.Parameters.AddWithValue("@Nombre", propietario.Nombre);
                    command.Parameters.AddWithValue("@Apellido", propietario.Apellido);
                    command.Parameters.AddWithValue("@Telefono", propietario.Telefono ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Email", propietario.Email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Id", propietario.Id);

                    connection.Open();
                    filasAfectadas = command.ExecuteNonQuery();
                }
            }
            return filasAfectadas;
        }

        public int Baja(int id)
        {
            int filasAfectadas = 0;
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "UPDATE Propietario SET EstadoActivo = 0, FechaBaja = CURRENT_TIMESTAMP WHERE Id = @id";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    filasAfectadas = command.ExecuteNonQuery();
                }
            }
            return filasAfectadas;
        }
    }
}