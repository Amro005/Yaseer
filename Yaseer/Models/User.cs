using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Yaseer.Models
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        public int DisabilityTypeId { get; set; }
        public DisabilityType? DisabilityType { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}

