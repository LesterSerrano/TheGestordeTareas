using GestordeTaras.EN;
using System.ComponentModel.DataAnnotations;

namespace GestordeTareas.UI.DTOs
{
    public class UsuarioEditDTO
    {
        [Required]
        public int Id { get; set; }
        [Display(Name = "Foto")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public string FotoPerfil { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre debe tener menos de 50 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido debe tener menos de 50 caracteres")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string NombreUsuario { get; set; } // Lo usas como correo

        [StringLength(15, ErrorMessage = "El teléfono debe tener menos de 15 caracteres")]
        public string Telefono { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        public int IdCargo { get; set; }
        public byte Status { get; set; }

    }
}