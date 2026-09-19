using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGrupoNN.Models
{
    public class Pago
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "La reserva no es válida.")]
        public int ReservaId { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio.")]
        [StringLength(150,
            ErrorMessage = "El concepto no puede superar los 150 caracteres.")]
        public string Concepto { get; set; } = "";

        [Required(ErrorMessage = "La fecha del pago es obligatoria.")]
        public DateTime FechaPago { get; set; } = DateTime.Today;

        [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El importe debe estar entre 0,01 y 99.999.999,99.")]
        public decimal Importe { get; set; }

        public bool EstadoActivo { get; set; } = true;

        public DateTime? FechaAnulacion { get; set; }

        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public int? CreadoPorId { get; set; }
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public int? AnuladoPorId { get; set; }
        public Usuario? CreadoPor { get; set; }
        public Usuario? AnuladoPor { get; set; }

        public Reserva? Reserva { get; set; }
    }
}