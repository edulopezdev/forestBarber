using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class ActualizarAtencionDto
    {
        [Required]
        public int Id { get; set; } // Necesario para validar que el id en URL y cuerpo coincidan

        [Required]
        public int ClienteId { get; set; }

        public int BarberoId { get; set; } // Lo puedes permitir opcional si quieres

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El total debe ser mayor o igual a cero")]
        public decimal Total { get; set; }

        [Required]
        public List<DetalleAtencionDto> DetalleAtencion { get; set; } =
            new List<DetalleAtencionDto>(); // Se inicializa para evitar null
    }
}
