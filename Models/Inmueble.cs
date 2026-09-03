using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InmobiliariaGrupoNN.Models
{
    public class Inmueble
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(100, ErrorMessage = "La dirección no puede superar los 100 caracteres.")]
        public string Direccion { get; set; } = "";

        [Range(1, 100, ErrorMessage = "La cantidad de ambientes debe ser mayor a 0.")]
        public int Ambientes { get; set; }

        [Range(1, 100, ErrorMessage = "El cupo debe ser mayor a 0.")]
        public int Cupo { get; set; }

        [Range(0.01, 99999999.99,
            ErrorMessage = "El precio por día debe ser mayor a 0.")]
        public decimal PrecioPorDia { get; set; }

        [Range(-90.0, 90.0,
            ErrorMessage = "La latitud debe estar entre -90 y 90.")]
        public decimal Latitud { get; set; }

        [Range(-180.0, 180.0,
            ErrorMessage = "La longitud debe estar entre -180 y 180.")]
        public decimal Longitud { get; set; }

        [Range(0.0, 100.0,
            ErrorMessage = "El porcentaje de reserva debe estar entre 0 y 100.")]
        public decimal PorcentajeReserva { get; set; }

        public bool Disponible { get; set; } = true;

        public bool EstadoActivo { get; set; } = true;

        public DateTime? FechaBaja { get; set; }

        public string? Portada { get; set; }

        public IFormFile? PortadaFile { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un propietario.")]
        public int PropietarioId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un tipo de inmueble.")]
        public int TipoInmuebleId { get; set; }

        public Propietario? Propietario { get; set; }

        public TipoInmueble? TipoInmueble { get; set; }

        public IList<Imagen> Imagenes { get; set; }
            = new List<Imagen>();
    }
}