using System.ComponentModel.DataAnnotations;

namespace Yaseer.Models
{
    public class Clinic
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}

