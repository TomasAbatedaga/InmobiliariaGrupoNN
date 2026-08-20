using InmobiliariaGrupoNN.Models;
using MySqlConnector;

namespace InmobiliariaGrupoNN.Repositories
{
    public class RepositorioInquilino : IRepositorioInquilino
    {
        private readonly string connectionString;

        public RepositorioInquilino(IConfiguration configuration)
        {
            connectionString = configuration
                .GetConnectionString("DefaultConnection")!;
        }

        public IList<Inquilino> ObtenerTodos()
        {
            var lista = new List<Inquilino>();

            using var connection = new MySqlConnection(connectionString);

            var sql = @"SELECT Id, Dni, Nombre, Apellido, Telefono, Email,
                               EstadoActivo, FechaAlta, FechaBaja
                        FROM Inquilino
                        WHERE EstadoActivo = 1";

            using var command = new MySqlCommand(sql, connection);

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Inquilino
                {
                    Id = reader.GetInt32("Id"),
                    Dni = reader.GetString("Dni"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),

                    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))
                        ? null
                        : reader.GetString("Telefono"),

                    Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                        ? null
                        : reader.GetString("Email"),

                    EstadoActivo = reader.GetBoolean("EstadoActivo"),
                    FechaAlta = reader.GetDateTime("FechaAlta"),

                    FechaBaja = reader.IsDBNull(reader.GetOrdinal("FechaBaja"))
                        ? null
                        : reader.GetDateTime("FechaBaja")
                });
            }

            return lista;
        }

        public Inquilino? ObtenerPorId(int id)
        {
            Inquilino? inquilino = null;

            using var connection = new MySqlConnection(connectionString);

            var sql = @"SELECT Id, Dni, Nombre, Apellido, Telefono, Email,
                               EstadoActivo, FechaAlta, FechaBaja
                        FROM Inquilino
                        WHERE Id = @id";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                inquilino = new Inquilino
                {
                    Id = reader.GetInt32("Id"),
                    Dni = reader.GetString("Dni"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),

                    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))
                        ? null
                        : reader.GetString("Telefono"),

                    Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                        ? null
                        : reader.GetString("Email"),

                    EstadoActivo = reader.GetBoolean("EstadoActivo"),
                    FechaAlta = reader.GetDateTime("FechaAlta"),

                    FechaBaja = reader.IsDBNull(reader.GetOrdinal("FechaBaja"))
                        ? null
                        : reader.GetDateTime("FechaBaja")
                };
            }

            return inquilino;
        }

        public int Alta(Inquilino inquilino)
        {
            using var connection = new MySqlConnection(connectionString);

            var sql = @"INSERT INTO Inquilino
                        (Dni, Nombre, Apellido, Telefono, Email)
                        VALUES
                        (@dni, @nombre, @Apellido, @telefono, @email)";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@dni", inquilino.Dni);
            command.Parameters.AddWithValue("@nombre", inquilino.Nombre);
            command.Parameters.AddWithValue("@apellido", inquilino.Apellido);
            command.Parameters.AddWithValue("@telefono", inquilino.Telefono);
            command.Parameters.AddWithValue("@email", inquilino.Email);

            connection.Open();

            return command.ExecuteNonQuery();
        }

        public int Modificacion(Inquilino inquilino)
        {
            using var connection = new MySqlConnection(connectionString);

            var sql = @"UPDATE Inquilino
                        SET Dni = @dni,
                            Nombre = @nombre,
                            Apellido = @apellido,
                            Telefono = @telefono,
                            Email = @email
                        WHERE Id = @id";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@id", inquilino.Id);
            command.Parameters.AddWithValue("@dni", inquilino.Dni);
            command.Parameters.AddWithValue("@nombre", inquilino.Nombre);
            command.Parameters.AddWithValue("@apellido", inquilino.Apellido);
            command.Parameters.AddWithValue("@telefono", inquilino.Telefono);
            command.Parameters.AddWithValue("@email", inquilino.Email);

            connection.Open();

            return command.ExecuteNonQuery();
        }

        public int Baja(int id)
        {
            using var connection = new MySqlConnection(connectionString);

            var sql = @"UPDATE Inquilino
                        SET EstadoActivo = 0,
                            FechaBaja = CURRENT_TIMESTAMP
                        WHERE Id = @id";

            using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();

            return command.ExecuteNonQuery();
        }
    }
}