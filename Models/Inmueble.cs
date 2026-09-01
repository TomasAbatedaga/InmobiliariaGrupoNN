using System;
using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGrupoNN.Models
{
    public class Inmueble
    {
        public int Id { get; set; }
        public string Direccion { get; set; }
        public int Ambientes { get; set; }
        public decimal Precio { get; set; }
        public bool EstadoActivo { get; set; }
        public int PropietarioId { get; set; }
        public int TipoInmuebleId { get; set; }
        public Propietario? Propietario { get; set; } 
        public TipoInmueble? TipoInmueble { get; set; }
    }
}
