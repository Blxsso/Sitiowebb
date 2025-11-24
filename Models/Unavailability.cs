// Sitiowebb/Models/Unavailability.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Sitiowebb.Models
{
    public class Unavailability
    {
        public int Id { get; set; }

        [EmailAddress, MaxLength(256)]
        public string? UserEmail { get; set; }        // email del empleado

        [MaxLength(32)]
        public string? Kind { get; set; }             // "sick","meeting","trip","halfday","vacation", etc.

        [Required]
        public DateTime StartDate { get; set; }       // fecha (o inicio)

        [Required]
        public DateTime EndDate { get; set; }         // fecha fin (igual a inicio si es 1 día)

        public bool IsHalfDay { get; set; } = false;  // true si es medio día

        [MaxLength(2)]
        public string? HalfSegment { get; set; }      // "AM"/"PM" (solo cuando IsHalfDay=true)

        [MaxLength(500)]
        public string? Justification { get; set; }    // texto opcional (sick obligatorio si así lo decides)

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow; // marca de creación
    }
}
