using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using InmobiliariaGrupoNN.Models;

namespace InmobiliariaGrupoNN.Repositories
{
    public class RepositorioUsuario : IRepositorioUsuario
    {
        private readonly string _connectionString;

        public RepositorioUsuario(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentException("No se encontró la cadena de conexión.");
        }

        public IList<Usuario> ObtenerTodos(int numeroPagina = 1, int tamanio = 10)
        {
            if (numeroPagina < 1) numeroPagina = 1;
            if (tamanio < 1) tamanio = 10;

            long offset = ((long)numeroPagina - 1) * tamanio;
            var usuarios = new List<Usuario>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "SELECT Id, Nombre, Apellido, Email, Clave, Avatar, Rol FROM Usuario ORDER BY Id LIMIT @tamanio OFFSET @offset";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@tamanio", tamanio);
                    command.Parameters.AddWithValue("@offset", offset);
                    
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usuarios.Add(new Usuario
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Email = reader.GetString(3),
                                Clave = reader.GetString(4),
                                Avatar = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Rol = (Rol)reader.GetInt32(6) 
                            });
                        }
                    }
                }
            }
            return usuarios;
        }
        public int ObtenerTotal()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM Usuario;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public Usuario? ObtenerPorId(int id)
        {
            Usuario? usuario = null;
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "SELECT Id, Nombre, Apellido, Email, Clave, Avatar, Rol FROM Usuario WHERE Id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Email = reader.GetString(3),
                                Clave = reader.GetString(4),
                                Avatar = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Rol = (Rol)reader.GetInt32(6)
                            };
                        }
                    }
                }
            }
            return usuario;
        }

        // MÉTODO CLAVE PARA EL LOGIN
        public Usuario? ObtenerPorEmail(string email)
        {
            Usuario? usuario = null;
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "SELECT Id, Nombre, Apellido, Email, Clave, Avatar, Rol FROM Usuario WHERE Email = @email";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Email = reader.GetString(3),
                                Clave = reader.GetString(4),
                                Avatar = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Rol = (Rol)reader.GetInt32(6)
                            };
                        }
                    }
                }
            }
            return usuario;
        }

        public int Alta(Usuario usuario)
        {
            int res = -1;
            using (var connection = new MySqlConnection(_connectionString))
            {
                // Verificamos que el email no exista antes de insertar
                if (ObtenerPorEmail(usuario.Email) != null)
                {
                    throw new InvalidOperationException("Ya existe un usuario registrado con este correo electrónico.");
                }

                string sql = @"INSERT INTO Usuario (Nombre, Apellido, Email, Clave, Avatar, Rol) 
                               VALUES (@nombre, @apellido, @email, @clave, @avatar, @rol); 
                               SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    command.Parameters.AddWithValue("@apellido", usuario.Apellido);
                    command.Parameters.AddWithValue("@email", usuario.Email);
                    command.Parameters.AddWithValue("@clave", usuario.Clave); // Más adelante veremos cómo encriptarla
                    command.Parameters.AddWithValue("@avatar", string.IsNullOrEmpty(usuario.Avatar) ? (object)DBNull.Value : usuario.Avatar);
                    command.Parameters.AddWithValue("@rol", (int)usuario.Rol); // Convertimos Enum a INT
                    
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    usuario.Id = res;
                }
            }
            return res;
        }

        public int Modificacion(Usuario usuario)
        {
            int res = -1;
            using (var connection = new MySqlConnection(_connectionString))
            {
                // Validación para que no le robe el email a otro usuario al editarse
                var existente = ObtenerPorEmail(usuario.Email);
                if (existente != null && existente.Id != usuario.Id)
                {
                    throw new InvalidOperationException("El correo electrónico ya está en uso por otra cuenta.");
                }

                string sql = @"UPDATE Usuario 
                               SET Nombre = @nombre, Apellido = @apellido, Email = @email, 
                                   Clave = @clave, Avatar = @avatar, Rol = @rol 
                               WHERE Id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    command.Parameters.AddWithValue("@apellido", usuario.Apellido);
                    command.Parameters.AddWithValue("@email", usuario.Email);
                    command.Parameters.AddWithValue("@clave", usuario.Clave);
                    command.Parameters.AddWithValue("@avatar", string.IsNullOrEmpty(usuario.Avatar) ? (object)DBNull.Value : usuario.Avatar);
                    command.Parameters.AddWithValue("@rol", (int)usuario.Rol);
                    command.Parameters.AddWithValue("@id", usuario.Id);
                    
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public int Baja(int id)
        {
            int res = -1;
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "DELETE FROM Usuario WHERE Id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }
    }
}