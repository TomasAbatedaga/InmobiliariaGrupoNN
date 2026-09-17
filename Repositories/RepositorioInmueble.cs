 using InmobiliariaGrupoNN.Models;
using MySqlConnector;

namespace InmobiliariaGrupoNN.Repositories
{
    public class RepositorioInmueble : IRepositorioInmueble
    {
        private readonly string _connectionString;

        public RepositorioInmueble(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión DefaultConnection");
        }

        public IList<Inmueble> ObtenerTodos(int numeroPagina = 1, int tamanio = 10)
        {
            if (numeroPagina < 1) numeroPagina = 1;
            if (tamanio < 1) tamanio = 10;

            int offset = (numeroPagina - 1) * tamanio;
            var inmuebles = new List<Inmueble>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT
                        i.Id,
                        i.Direccion,
                        i.Ambientes,
                        i.Cupo,
                        i.PrecioPorDia,
                        i.Latitud,
                        i.Longitud,
                        i.PorcentajeReserva,
                        i.Disponible,
                        i.EstadoActivo,
                        i.FechaBaja,
                        i.Portada,
                        i.PropietarioId,
                        i.TipoInmuebleId,
                        p.Nombre,
                        p.Apellido,
                        t.Nombre
                    FROM Inmueble i
                    INNER JOIN Propietario p
                        ON i.PropietarioId = p.Id
                    INNER JOIN TipoInmueble t
                        ON i.TipoInmuebleId = t.Id
                    WHERE i.EstadoActivo = 1
                    ORDER BY i.Id
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
                            inmuebles.Add(MapearInmueble(reader));
                        }
                    }
                }
            }

            return inmuebles;
        }
        public int ObtenerTotal()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM Inmueble;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        // La búsqueda y el conteo comparten exactamente los mismos filtros.
        private const string FiltroDisponibles = @"
            FROM Inmueble i
            INNER JOIN Propietario p ON p.Id = i.PropietarioId
            INNER JOIN TipoInmueble t ON t.Id = i.TipoInmuebleId
            WHERE i.EstadoActivo = TRUE AND i.Disponible = TRUE
              AND (@CupoMinimo IS NULL OR i.Cupo >= @CupoMinimo)
              AND (@Tipo IS NULL OR t.Nombre LIKE @Tipo)
              AND NOT EXISTS (
                  SELECT 1 FROM Reserva r
                  WHERE r.InmuebleId = i.Id
                    AND r.FechaInicio < @FechaFin
                    AND r.FechaFin > @FechaInicio
              )";

        public IList<Inmueble> BuscarDisponibles(DateTime fechaInicio, DateTime fechaFin,
            int? cupoMinimo, string? tipo, int numeroPagina = 1, int tamanio = 10)
        {
            numeroPagina = Math.Max(1, numeroPagina);
            if (tamanio != 5 && tamanio != 10 && tamanio != 20) tamanio = 10;
            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(@"SELECT
                i.Id, i.Direccion, i.Ambientes, i.Cupo, i.PrecioPorDia,
                i.Latitud, i.Longitud, i.PorcentajeReserva, i.Disponible,
                i.EstadoActivo, i.FechaBaja, i.Portada, i.PropietarioId,
                i.TipoInmuebleId, p.Nombre, p.Apellido, t.Nombre"
                + FiltroDisponibles + " ORDER BY i.Id LIMIT @Tamanio OFFSET @Offset", connection);
            AgregarParametrosDisponibilidad(command, fechaInicio, fechaFin, cupoMinimo, tipo);
            command.Parameters.AddWithValue("@Tamanio", tamanio);
            command.Parameters.AddWithValue("@Offset", ((long)numeroPagina - 1) * tamanio);
            connection.Open();
            using var reader = command.ExecuteReader();
            var inmuebles = new List<Inmueble>();
            while (reader.Read()) inmuebles.Add(MapearInmueble(reader));
            return inmuebles;
        }

        public int ObtenerCantidadDisponibles(DateTime fechaInicio, DateTime fechaFin,
            int? cupoMinimo, string? tipo)
        {
            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand("SELECT COUNT(*) " + FiltroDisponibles, connection);
            AgregarParametrosDisponibilidad(command, fechaInicio, fechaFin, cupoMinimo, tipo);
            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar());
        }

        private static void AgregarParametrosDisponibilidad(MySqlCommand command,
            DateTime fechaInicio, DateTime fechaFin, int? cupoMinimo, string? tipo)
        {
            fechaInicio = fechaInicio.Date;
            fechaFin = fechaFin.Date;
            if (fechaInicio < new DateTime(1000, 1, 1) || fechaFin <= fechaInicio)
                throw new ArgumentException("Debe indicar un período de fechas válido.");
            if (cupoMinimo.HasValue && cupoMinimo.Value < 1)
                throw new ArgumentException("El cupo mínimo debe ser mayor a cero.");
            command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
            command.Parameters.AddWithValue("@FechaFin", fechaFin);
            command.Parameters.AddWithValue("@CupoMinimo", (object?)cupoMinimo ?? DBNull.Value);
            command.Parameters.AddWithValue("@Tipo", string.IsNullOrWhiteSpace(tipo)
                ? DBNull.Value : (object)("%" + tipo.Trim() + "%"));
        }
        public Inmueble? ObtenerPorId(int id)
        {
            Inmueble? inmueble = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT
                        i.Id,
                        i.Direccion,
                        i.Ambientes,
                        i.Cupo,
                        i.PrecioPorDia,
                        i.Latitud,
                        i.Longitud,
                        i.PorcentajeReserva,
                        i.Disponible,
                        i.EstadoActivo,
                        i.FechaBaja,
                        i.Portada,
                        i.PropietarioId,
                        i.TipoInmuebleId,
                        p.Nombre,
                        p.Apellido,
                        t.Nombre
                    FROM Inmueble i
                    INNER JOIN Propietario p
                        ON i.PropietarioId = p.Id
                    INNER JOIN TipoInmueble t
                        ON i.TipoInmuebleId = t.Id
                    WHERE i.Id = @Id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            inmueble = MapearInmueble(reader);
                        }
                    }
                }
            }

            return inmueble;
        }

        public int Alta(Inmueble inmueble)
        {
            int id;

            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO Inmueble
                    (
                        Direccion,
                        Ambientes,
                        Cupo,
                        PrecioPorDia,
                        Latitud,
                        Longitud,
                        PorcentajeReserva,
                        Disponible,
                        PropietarioId,
                        TipoInmuebleId
                    )
                    VALUES
                    (
                        @Direccion,
                        @Ambientes,
                        @Cupo,
                        @PrecioPorDia,
                        @Latitud,
                        @Longitud,
                        @PorcentajeReserva,
                        @Disponible,
                        @PropietarioId,
                        @TipoInmuebleId
                    );

                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Direccion",
                        inmueble.Direccion);

                    command.Parameters.AddWithValue(
                        "@Ambientes",
                        inmueble.Ambientes);

                    command.Parameters.AddWithValue(
                        "@Cupo",
                        inmueble.Cupo);

                    command.Parameters.AddWithValue(
                        "@PrecioPorDia",
                        inmueble.PrecioPorDia);

                    command.Parameters.AddWithValue(
                        "@Latitud",
                        inmueble.Latitud);

                    command.Parameters.AddWithValue(
                        "@Longitud",
                        inmueble.Longitud);

                    command.Parameters.AddWithValue(
                        "@PorcentajeReserva",
                        inmueble.PorcentajeReserva);

                    command.Parameters.AddWithValue(
                        "@Disponible",
                        inmueble.Disponible);

                    command.Parameters.AddWithValue(
                        "@PropietarioId",
                        inmueble.PropietarioId);

                    command.Parameters.AddWithValue(
                        "@TipoInmuebleId",
                        inmueble.TipoInmuebleId);

                    connection.Open();

                    id = Convert.ToInt32(command.ExecuteScalar());

                    inmueble.Id = id;
                }
            }

            return id;
        }

        public int Modificacion(Inmueble inmueble)
        {
            int filasAfectadas;

            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Inmueble
                    SET
                        Direccion = @Direccion,
                        Ambientes = @Ambientes,
                        Cupo = @Cupo,
                        PrecioPorDia = @PrecioPorDia,
                        Latitud = @Latitud,
                        Longitud = @Longitud,
                        PorcentajeReserva = @PorcentajeReserva,
                        Disponible = @Disponible,
                        PropietarioId = @PropietarioId,
                        TipoInmuebleId = @TipoInmuebleId
                    WHERE Id = @Id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Direccion",
                        inmueble.Direccion);

                    command.Parameters.AddWithValue(
                        "@Ambientes",
                        inmueble.Ambientes);

                    command.Parameters.AddWithValue(
                        "@Cupo",
                        inmueble.Cupo);

                    command.Parameters.AddWithValue(
                        "@PrecioPorDia",
                        inmueble.PrecioPorDia);

                    command.Parameters.AddWithValue(
                        "@Latitud",
                        inmueble.Latitud);

                    command.Parameters.AddWithValue(
                        "@Longitud",
                        inmueble.Longitud);

                    command.Parameters.AddWithValue(
                        "@PorcentajeReserva",
                        inmueble.PorcentajeReserva);

                    command.Parameters.AddWithValue(
                        "@Disponible",
                        inmueble.Disponible);

                    command.Parameters.AddWithValue(
                        "@PropietarioId",
                        inmueble.PropietarioId);

                    command.Parameters.AddWithValue(
                        "@TipoInmuebleId",
                        inmueble.TipoInmuebleId);

                    command.Parameters.AddWithValue(
                        "@Id",
                        inmueble.Id);

                    connection.Open();

                    filasAfectadas = command.ExecuteNonQuery();
                }
            }

            return filasAfectadas;
        }

        public int Baja(int id)
        {
            int filasAfectadas;

            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Inmueble
                    SET
                        EstadoActivo = 0,
                        FechaBaja = CURRENT_TIMESTAMP
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

        public int ModificarPortada(int id, string? ruta)
        {
            int filasAfectadas;

            using (var connection = new MySqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE Inmueble
                    SET Portada = @Portada
                    WHERE Id = @Id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Portada",
                        string.IsNullOrEmpty(ruta)
                            ? DBNull.Value
                            : ruta);

                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();

                    filasAfectadas = command.ExecuteNonQuery();
                }
            }

            return filasAfectadas;
        }

        private Inmueble MapearInmueble(MySqlDataReader reader)
        {
            return new Inmueble
            {
                Id = reader.GetInt32(0),

                Direccion = reader.GetString(1),

                Ambientes = reader.GetInt32(2),

                Cupo = reader.GetInt32(3),

                PrecioPorDia = reader.GetDecimal(4),

                Latitud = reader.GetDecimal(5),

                Longitud = reader.GetDecimal(6),

                PorcentajeReserva = reader.GetDecimal(7),

                Disponible = reader.GetBoolean(8),

                EstadoActivo = reader.GetBoolean(9),

                FechaBaja = reader.IsDBNull(10)
                    ? null
                    : reader.GetDateTime(10),

                Portada = reader.IsDBNull(11)
                    ? null
                    : reader.GetString(11),

                PropietarioId = reader.GetInt32(12),

                TipoInmuebleId = reader.GetInt32(13),

                Propietario = new Propietario
                {
                    Id = reader.GetInt32(12),
                    Nombre = reader.GetString(14),
                    Apellido = reader.GetString(15)
                },

                TipoInmueble = new TipoInmueble
                {
                    Id = reader.GetInt32(13),
                    Nombre = reader.GetString(16)
                }
            };
        }
    }
}