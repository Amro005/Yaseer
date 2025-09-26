using System.ComponentModel.DataAnnotations;

namespace Yaseer.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [Required]
        public TimeSpan AppointmentEndTime { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsConfirmed { get; set; } = false;

        public bool NeedsTransport { get; set; } = false;

        [StringLength(300)]
        public string? TransportAddress { get; set; }

        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }

        public int ClinicId { get; set; }
        public Clinic? Clinic { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
