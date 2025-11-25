using System;
using System.ComponentModel.DataAnnotations;

namespace Sitiowebb.Models
{
    public class VacationRequest
    {
        // Clave primaria de la tabla
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        // Tipo real de la solicitud: vacation, sick, halfday
        [Required]
        public string Kind { get; set; } = string.Empty;

        // Fechas: ahora con zona horaria (DateTimeOffset) para PostgreSQL
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }

        public DateTimeOffset CreatedUtc { get; set; }

        // Comentario del usuario (justificación)
        public string? UserComment { get; set; }

        // Estado (Pending / Approved / Denied)
        public RequestStatus Status { get; set; }

        // Campos del manager
        public string ManagerComment { get; set; } = string.Empty;
        public DateTimeOffset? DecidedUtc { get; set; }
    }
}