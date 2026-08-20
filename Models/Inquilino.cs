using System;
using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGrupoNN.Models
{
    public class Inquilino
    {
        public int Id { get; set; }

        [Required]
        public string Dni { get; set; } = "";

        [Required]
        public string? Nombre { get; set; } = "";
        public string? Apellido { get; set; } = "";

        public string? Telefono { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public bool EstadoActivo { get; set; }

        public DateTime FechaAlta { get; set; }

        public DateTime? FechaBaja { get; set; }
    }
}