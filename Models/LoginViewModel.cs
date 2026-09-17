using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGrupoNN.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo invalido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contrasenia es obligatoria")]
        [DataType(DataType.Password)]
        public string Clave { get; set; } = "";
    }
}