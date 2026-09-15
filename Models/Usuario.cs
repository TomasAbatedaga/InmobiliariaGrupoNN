using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGrupoNN.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = "";
        
        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; } = "";
        
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo invalido")]
        public string Email { get; set; } = "";
        
        [Required(ErrorMessage = "La contrasenia es obligatoria")]
        public string Clave { get; set; } = "";
        
        public string? Avatar { get; set; }
        
        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public Rol Rol { get; set; }
        
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}