using System.ComponentModel.DataAnnotations;

namespace Yaseer.Models
{
    public class ClinicCreateRequest
    {
        [Display(Name = "اسم العيادة")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [StringLength(200, ErrorMessage = "الحد الأقصى {1} حرفاً")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الوصف")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [StringLength(500, ErrorMessage = "الحد الأقصى {1} حرفاً")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "العنوان")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [StringLength(300, ErrorMessage = "الحد الأقصى {1} حرفاً")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "التخصص")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [StringLength(200, ErrorMessage = "الحد الأقصى {1} حرفاً")]
        public string Specialization { get; set; } = string.Empty;

        [Display(Name = "رابط الصورة (اختياري)")]
        [StringLength(500, ErrorMessage = "الحد الأقصى {1} حرفاً")]
        public string? ImageUrl { get; set; }

        [Display(Name = "رقم الهاتف (اختياري)")]
        [StringLength(20, ErrorMessage = "الحد الأقصى {1} حرفاً")]
        public string? PhoneNumber { get; set; }
    }
}

