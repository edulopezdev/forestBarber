using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Turno
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int BarberoId { get; set; }

        [Required]
        public int EstadoId { get; set; }

        // Propiedades de navegación
        public virtual Usuario? Cliente { get; set; }
        public virtual Usuario? Barbero { get; set; }
        public virtual EstadoTurno? EstadoTurno { get; set; }
    }
}
