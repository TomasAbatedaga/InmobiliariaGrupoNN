using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaGrupoNN.Models
{
    public class Reserva {
        public int Id { get; set; }
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public bool EstadoActivo { get; set; } = true;
        public int InmuebleId { get; set; }
        public int InquilinoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal MontoPorDia { get; set; }
        public decimal MontoTotal => (FechaFin - FechaInicio).Days * MontoPorDia;
        public Inmueble? Inmueble { get; set; }
        public Inquilino? Inquilino { get; set; }
        public int? CreadoPorId { get; set; }
        [ForeignKey("CreadoPorId")]
        public Usuario? CreadoPor { get; set; }
        public int? AnuladoPorId { get; set; }
        
        [ForeignKey("AnuladoPorId")]
        public Usuario? AnuladoPor { get; set; }
    }
}