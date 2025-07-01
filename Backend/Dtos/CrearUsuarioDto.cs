using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class CrearUsuarioDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public string Email { get; set; } = null!;

        public string? Telefono { get; set; }

        public string? Avatar { get; set; }

        [Required(ErrorMessage = "El RolId es obligatorio.")]
        public int RolId { get; set; }

        public bool AccedeAlSistema { get; set; } = false;

        public string? Password { get; set; }
    }
}
