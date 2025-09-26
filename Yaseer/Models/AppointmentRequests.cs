namespace Yaseer.Models
{
    public class AppointmentCheckRequest
    {
        public int ClinicId { get; set; }
        public string AppointmentDate { get; set; } = string.Empty;
        public string AppointmentTime { get; set; } = string.Empty;
        public string AppointmentEndTime { get; set; } = string.Empty;
    }

    public class AppointmentCreateRequest
    {
        public int ClinicId { get; set; }
        public string AppointmentDate { get; set; } = string.Empty;
        public string AppointmentTime { get; set; } = string.Empty;
        public string AppointmentEndTime { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool NeedsTransport { get; set; } = false;
        public string? TransportAddress { get; set; }
    }
}

