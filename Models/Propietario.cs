using System;

namespace InmobiliariaGrupoNN.Models
{
    public class Propietario
    {
        public int Id { get; set; }
        public string Dni { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public bool EstadoActivo { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBaja { get; set; }
    }
}