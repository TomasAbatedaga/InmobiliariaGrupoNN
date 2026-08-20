using System;
using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGrupoNN.Models
{
    public class Propietario
    {
        public int Id { get; set; }

        [Required]
        public string Dni { get; set; } = string.Empty;
        [Required]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        public string Apellido { get; set; } = string.Empty;
        public string? Telefono { get; set; } 
        [Required]
        public string? Email { get; set; }
        public bool EstadoActivo { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBaja { get; set; }
    }
}