using System;
using System.Collections.Generic;
using MySqlConnector;
using InmobiliariaGrupoNN.Models;
using Microsoft.Extensions.Configuration;

namespace InmobiliariaGrupoNN.Repositories
{
    public class RepositorioTipoInmueble : IRepositorioTipoInmueble
    {
        private readonly string _connectionString;

        public RepositorioTipoInmueble(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentException("No se encontro la cadena de conexion en el appsettings.");
        }

        public IList<TipoInmueble> ObtenerTodos(int numeroPagina = 1, int tamanio = 10)
        {
            if (numeroPagina < 1) numeroPagina = 1;
            if (tamanio < 1) tamanio = 10;

            int offset = (numeroPagina - 1) * tamanio;
            var tipos = new List<TipoInmueble>();
            
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string sql = "SELECT Id, Nombre FROM TipoInmueble LIMIT @tamanio OFFSET @offset";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@tamanio", tamanio);
                        command.Parameters.AddWithValue("@offset", offset);
                        
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tipos.Add(new TipoInmueble
                                {
                                    Id = reader.GetInt32(0),
                                    Nombre = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Hubo un error al intentar obtener los tipos de inmueble paginados de la base de datos.", ex);
            }
            
            return tipos;
        }

        public TipoInmueble? ObtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser mayor a cero");
            }
            TipoInmueble? tipo = null;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string sql = "SELECT Id, Nombre FROM TipoInmueble WHERE Id = @id";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tipo = new TipoInmueble
                                {
                                    Id = reader.GetInt32(0),
                                    Nombre = reader.GetString(1)
                                };
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Error al buscar el tipo de inmueble con ID {id}.", ex);
            }
            
            return tipo;
        }

        public int Alta(TipoInmueble tipo)
        {
            if (tipo == null || string.IsNullOrWhiteSpace(tipo.Nombre))
            {
                throw new ArgumentException("El nombre del tipo de inmueble es obligatorio y no puede estar vacio.");
            }
            int res = -1;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string sql = "INSERT INTO TipoInmueble (Nombre) VALUES (@nombre); SELECT LAST_INSERT_ID();";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@nombre", tipo.Nombre.Trim());
                        connection.Open();
                        res = Convert.ToInt32(command.ExecuteScalar());
                        tipo.Id = res;
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al intentar guardar el nuevo tipo de inmueble.", ex);
            }
            return res;
        }

        public int Modificacion(TipoInmueble tipo)
        {
            if (tipo == null || tipo.Id <= 0)
            {
                throw new ArgumentException("Datos invalidos para modificar el tipo de inmueble.");
            }
            if (string.IsNullOrWhiteSpace(tipo.Nombre))
            {
                throw new ArgumentException("El nombre del tipo de inmueble no puede quedar vacio.");
            }

            int res = -1;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    string sql = "UPDATE TipoInmueble SET Nombre = @nombre WHERE Id = @id";
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@nombre", tipo.Nombre.Trim());
                        command.Parameters.AddWithValue("@id", tipo.Id);
                        
                        connection.Open();
                        res = command.ExecuteNonQuery(); 
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Error al intentar actualizar el tipo de inmueble.", ex);
            }
            return res;
        }

        public int Baja(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID proporcionado no es valido para la eliminacion.");
            }

            int res = -1;
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "DELETE FROM TipoInmueble WHERE Id = @id";
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
                        if (ex.Number == 1451)
                        {
                            throw new InvalidOperationException("No se puede eliminar este Tipo de Inmueble porque existen Inmuebles asociados a el.");
                        }
                        
                        throw new Exception("Error al intentar eliminar el tipo de inmueble de la base de datos.", ex);
                    }
                }
            }
            return res;
        }
    }
}