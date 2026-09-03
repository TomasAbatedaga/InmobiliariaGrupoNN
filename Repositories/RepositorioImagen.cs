using InmobiliariaGrupoNN.Models;
using MySqlConnector;

namespace InmobiliariaGrupoNN.Repositories
{
    public class RepositorioImagen : IRepositorioImagen
    {
        private readonly string _connectionString;

        public RepositorioImagen(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión DefaultConnection");
        }

        public int Alta(Imagen imagen)
        {
            int filasAfectadas = 0;

            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO Imagen
                    (InmuebleId, Url)
                    VALUES
                    (@InmuebleId, @Url)";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@InmuebleId",
                        imagen.InmuebleId);

                    command.Parameters.AddWithValue(
                        "@Url",
                        imagen.Url);

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
                string sql = @"
                    DELETE FROM Imagen
                    WHERE Id = @Id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();

                    filasAfectadas = command.ExecuteNonQuery();
                }
            }

            return filasAfectadas;
        }

        public int Modificacion(Imagen imagen)
        {
            int filasAfectadas = 0;

            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Imagen
                    SET Url = @Url
                    WHERE Id = @Id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Url",
                        imagen.Url);

                    command.Parameters.AddWithValue(
                        "@Id",
                        imagen.Id);

                    connection.Open();

                    filasAfectadas = command.ExecuteNonQuery();
                }
            }

            return filasAfectadas;
        }

        public Imagen? ObtenerPorId(int id)
        {
            Imagen? imagen = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT Id, InmuebleId, Url
                    FROM Imagen
                    WHERE Id = @Id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            imagen = new Imagen
                            {
                                Id = reader.GetInt32(0),
                                InmuebleId = reader.GetInt32(1),
                                Url = reader.GetString(2)
                            };
                        }
                    }
                }
            }

            return imagen;
        }

        public IList<Imagen> BuscarPorInmueble(int inmuebleId)
        {
            var imagenes = new List<Imagen>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT Id, InmuebleId, Url
                    FROM Imagen
                    WHERE InmuebleId = @InmuebleId
                    ORDER BY Id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@InmuebleId",
                        inmuebleId);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            imagenes.Add(new Imagen
                            {
                                Id = reader.GetInt32(0),
                                InmuebleId = reader.GetInt32(1),
                                Url = reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return imagenes;
        }
    }
}