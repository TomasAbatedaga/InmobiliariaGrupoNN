using InmobiliariaGrupoNN.Models;
using MySqlConnector;

namespace InmobiliariaGrupoNN.Repositories
{
    public class RepositorioPago : IRepositorioPago
    {
        private readonly string _connectionString;

        public RepositorioPago(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión DefaultConnection.");
        }

        public IList<Pago> ObtenerPorReserva(int reservaId, int numeroPagina = 1, int tamanio = 10)
        {
            ValidarId(reservaId);
            numeroPagina = Math.Max(1, numeroPagina);
            if (tamanio < 1) tamanio = 10;
            var pagos = new List<Pago>();
            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(@"SELECT Id, ReservaId, Concepto, FechaPago, Importe, EstadoActivo, FechaAnulacion, CreadoPorId, AnuladoPorId
                FROM Pago WHERE ReservaId = @ReservaId ORDER BY Id DESC LIMIT @Tamanio OFFSET @Offset", connection);
            command.Parameters.AddWithValue("@ReservaId", reservaId);
            command.Parameters.AddWithValue("@Tamanio", tamanio);
            command.Parameters.AddWithValue("@Offset", ((long)numeroPagina - 1) * tamanio);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read()) pagos.Add(MapearPago(reader));
            return pagos;
        }

        public int ObtenerCantidadPorReserva(int reservaId)
        {
            ValidarId(reservaId);
            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand("SELECT COUNT(*) FROM Pago WHERE ReservaId = @ReservaId", connection);
            command.Parameters.AddWithValue("@ReservaId", reservaId);
            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public Pago? ObtenerPorId(int id)
        {
            ValidarId(id);
            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(@"SELECT Id, ReservaId, Concepto, FechaPago, Importe, EstadoActivo, FechaAnulacion, CreadoPorId, AnuladoPorId
                FROM Pago WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearPago(reader) : null;
        }

        public int Alta(Pago pago)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return Alta(pago, connection, null);
        }

        // El llamador conserva la propiedad de la conexión y de la transacción.
        public int Alta(Pago pago, MySqlConnection connection, MySqlTransaction? transaction)
        {
            ArgumentNullException.ThrowIfNull(pago);
            ValidarId(pago.ReservaId);
            ValidarId(pago.CreadoPorId ?? 0);
            string concepto = ValidarConcepto(pago.Concepto);
            if (pago.FechaPago.Date < new DateTime(1000, 1, 1))
                throw new ArgumentException("La fecha del pago no es válida para MySQL.");
            if (pago.Importe < 0.01m || pago.Importe > 99999999.99m)
                throw new ArgumentException("El importe debe estar entre 0,01 y 99.999.999,99.");
            if (decimal.Round(pago.Importe, 2) != pago.Importe)
                throw new ArgumentException("El importe debe tener como máximo dos decimales.");

            using var command = new MySqlCommand(@"INSERT INTO Pago
                (ReservaId, Concepto, FechaPago, Importe, EstadoActivo, FechaAnulacion, CreadoPorId)
                VALUES (@ReservaId, @Concepto, @FechaPago, @Importe, TRUE, NULL, @CreadoPorId);
                SELECT LAST_INSERT_ID();", connection, transaction);
            command.Parameters.AddWithValue("@ReservaId", pago.ReservaId);
            command.Parameters.AddWithValue("@Concepto", concepto);
            command.Parameters.AddWithValue("@FechaPago", pago.FechaPago.Date);
            command.Parameters.AddWithValue("@Importe", pago.Importe);
            command.Parameters.AddWithValue("@CreadoPorId", pago.CreadoPorId);
            
            pago.Id = Convert.ToInt32(command.ExecuteScalar());
            pago.Concepto = concepto;
            pago.FechaPago = pago.FechaPago.Date;
            pago.EstadoActivo = true;
            pago.FechaAnulacion = null;
            pago.AnuladoPorId = null;
            return pago.Id;
        }

        public int ModificarConcepto(int id, string concepto)
        {
            ValidarId(id);
            concepto = ValidarConcepto(concepto);
            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(@"UPDATE Pago SET Concepto = @Concepto
                WHERE Id = @Id AND EstadoActivo = TRUE", connection);
            command.Parameters.AddWithValue("@Concepto", concepto);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public int Anular(int id, int anuladoPorId)
        {
            ValidarId(id);
            ValidarId(anuladoPorId);
            using var connection = new MySqlConnection(_connectionString);
            using var command = new MySqlCommand(@"UPDATE Pago SET EstadoActivo = FALSE,
                FechaAnulacion = CURRENT_TIMESTAMP, AnuladoPorId = @AnuladoPorId WHERE Id = @Id AND EstadoActivo = TRUE", connection);
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@AnuladoPorId", anuladoPorId);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        private static Pago MapearPago(MySqlDataReader reader) => new Pago
        {
            Id = reader.GetInt32("Id"),
            ReservaId = reader.GetInt32("ReservaId"),
            Concepto = reader.GetString("Concepto"),
            FechaPago = reader.GetDateTime("FechaPago"),
            Importe = reader.GetDecimal("Importe"),
            EstadoActivo = reader.GetBoolean("EstadoActivo"),
            CreadoPorId = reader.IsDBNull(reader.GetOrdinal("CreadoPorId")) ? null : reader.GetInt32("CreadoPorId"),
            AnuladoPorId = reader.IsDBNull(reader.GetOrdinal("AnuladoPorId")) ? null : reader.GetInt32("AnuladoPorId"),
            FechaAnulacion = reader.IsDBNull(reader.GetOrdinal("FechaAnulacion"))
                ? null : reader.GetDateTime("FechaAnulacion")
        };

        private static void ValidarId(int id)
        {
            if (id <= 0) throw new ArgumentException("El identificador debe ser mayor a cero.");
        }

        private static string ValidarConcepto(string concepto)
        {
            if (string.IsNullOrWhiteSpace(concepto))
                throw new ArgumentException("El concepto es obligatorio.");
            concepto = concepto.Trim();
            if (concepto.Length > 150)
                throw new ArgumentException("El concepto no puede superar los 150 caracteres.");
            return concepto;
        }
    }
}
