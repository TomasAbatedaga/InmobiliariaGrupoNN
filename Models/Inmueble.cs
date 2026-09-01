using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGrupoNN.Models
{
    public class Inmueble
    {
        public int Id { get; set; }

        [Required]
        public string Direccion { get; set; } = "";

        [Range(1, int.MaxValue)]
        public int Ambientes { get; set; }

        [Range(1, int.MaxValue)]
        public int Cupo { get; set; }

        [Range(typeof(decimal), "0.01", "999999999.99")]
        public decimal PrecioPorDia { get; set; }

        public decimal Latitud { get; set; }

        public decimal Longitud { get; set; }

        [Range(typeof(decimal), "0", "100")]
        public decimal PorcentajeReserva { get; set; }

        public bool Disponible { get; set; } = true;

        public bool EstadoActivo { get; set; } = true;

        public DateTime? FechaBaja { get; set; }

        public int PropietarioId { get; set; }

        public int TipoInmuebleId { get; set; }

        public Propietario? Propietario { get; set; }

        public TipoInmueble? TipoInmueble { get; set; }
    }
}