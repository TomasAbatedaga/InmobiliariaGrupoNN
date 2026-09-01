using System;
using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGrupoNN.Models
{
    public class Reserva {
        public int Id { get; set; }
        public int InmuebleId { get; set; }
        public int InquilinoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal Monto { get; set; }
        public Inmueble? Inmueble { get; set; }
        public Inquilino? Inquilino { get; set; }
    }
}