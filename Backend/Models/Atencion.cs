using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using backend.Data;
using backend.Models;

namespace backend.Models
{
    public class Atencion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int BarberoId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal Total { get; set; }

        public int? TurnoId { get; set; }

        public Usuario Cliente { get; set; } = null!;
        public Usuario Barbero { get; set; } = null!;
        public ICollection<DetalleAtencion> DetalleAtencion { get; set; } =
            new List<DetalleAtencion>();
    }
}
