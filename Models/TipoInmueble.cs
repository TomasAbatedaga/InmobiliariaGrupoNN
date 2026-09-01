using System;
using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGrupoNN.Models
{
    public class TipoInmueble {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
    }
}