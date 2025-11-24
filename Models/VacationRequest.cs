using System;
using System.ComponentModel.DataAnnotations;

namespace Sitiowebb.Models
{
    public class VacationRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        // Tipo real de la solicitud: vacation, sick, halfday
        [Required]
        public string Kind { get; set; } = string.Empty;

        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public DateTime CreatedUtc { get; set; }

        // 🔹 Comentario del usuario (justificación)
        public string? UserComment { get; set; }

        // Estado (Pending / Approved / Denied)
        public RequestStatus Status { get; set; }

        // Campos del manager
        public string ManagerComment { get; set; } = string.Empty;
        public DateTime? DecidedUtc { get; set; }
    }
}