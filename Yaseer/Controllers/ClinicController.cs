using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using Yaseer.Data;
using Yaseer.Models;

namespace Yaseer.Controllers
{
    public class ClinicController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _env;

        public ClinicController(ApplicationDbContext context, UserManager<User> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
            {
                return NotFound();
            }
            return View(clinic);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BookAppointment([FromBody] AppointmentCreateRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, message = "طلب غير صالح" });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }

                if (!DateTime.TryParseExact(request.AppointmentDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate)
                    || !TimeSpan.TryParse(request.AppointmentTime, out var time)
                    || !TimeSpan.TryParse(request.AppointmentEndTime, out var endTime))
                {
                    return Json(new { success = false, message = "تاريخ أو وقت غير صحيح" });
                }

                if (endTime <= time)
                {
                    return Json(new { success = false, message = "وقت الانتهاء يجب أن يكون بعد وقت البداية" });
                }

                var existingAppointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.ClinicId == request.ClinicId &&
                                              a.AppointmentDate.Date == parsedDate.Date &&
                                              (time < a.AppointmentEndTime && endTime > a.AppointmentTime));

                if (existingAppointment != null)
                {
                    return Json(new { success = false, message = "هذا الموعد محجوز مسبقاً" });
                }

                var appointment = new Appointment
                {
                    ClinicId = request.ClinicId,
                    UserId = user.Id,
                    AppointmentDate = parsedDate,
                    AppointmentTime = time,
                    AppointmentEndTime = endTime,
                    Notes = request.Notes,
                    NeedsTransport = request.NeedsTransport,
                    TransportAddress = string.IsNullOrWhiteSpace(request.TransportAddress) ? null : request.TransportAddress
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "تم حجز الموعد بنجاح" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "تعذر إتمام الحجز حالياً" });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Consumes("application/json")]
        public async Task<IActionResult> CheckAvailability([FromBody] AppointmentCheckRequest request)
        {
            if (request == null)
            {
                return Json(new { available = false, message = "Invalid request" });
            }
            if (!DateTime.TryParseExact(request.AppointmentDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate) ||
                !TimeSpan.TryParse(request.AppointmentTime, out var time) ||
                !TimeSpan.TryParse(request.AppointmentEndTime, out var endTime))
            {
                return Json(new { available = false, message = "Invalid time" });
            }

            if (endTime <= time)
            {
                return Json(new { available = false, message = "Invalid range" });
            }

            var exists = await _context.Appointments
                .AnyAsync(a => a.ClinicId == request.ClinicId && a.AppointmentDate.Date == parsedDate.Date && (time < a.AppointmentEndTime && endTime > a.AppointmentTime));

            if (exists)
            {
                return Json(new { available = false, message = "Slot Unavailable" });
            }

            return Json(new { available = true, message = "Slot Available" });
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            ViewBag.IsEdit = false;
            return View(new ClinicCreateRequest());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClinicCreateRequest request, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            string? imageUrl = request.ImageUrl;
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                imageUrl = $"/uploads/{fileName}";
            }

            var clinic = new Clinic
            {
                Name = request.Name,
                Description = request.Description,
                Address = request.Address,
                Specialization = request.Specialization,
                ImageUrl = imageUrl,
                PhoneNumber = request.PhoneNumber
            };

            _context.Clinics.Add(clinic);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
            {
                return NotFound();
            }

            var vm = new ClinicCreateRequest
            {
                Name = clinic.Name,
                Description = clinic.Description,
                Address = clinic.Address,
                Specialization = clinic.Specialization,
                ImageUrl = clinic.ImageUrl,
                PhoneNumber = clinic.PhoneNumber
            };

            ViewBag.IsEdit = true;
            ViewBag.ClinicId = id;
            return View("Create", vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClinicCreateRequest request, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsEdit = true;
                ViewBag.ClinicId = id;
                return View("Create", request);
            }

            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
            {
                return NotFound();
            }

            clinic.Name = request.Name;
            clinic.Description = request.Description;
            clinic.Address = request.Address;
            clinic.Specialization = request.Specialization;
            clinic.PhoneNumber = request.PhoneNumber;

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                clinic.ImageUrl = $"/uploads/{fileName}";
            }
            else if (!string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                clinic.ImageUrl = request.ImageUrl;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Dashboard");
        }
        public async Task<IActionResult> GetAvailableTimes(int clinicId, DateTime date)
        {
            var bookedTimes = await _context.Appointments
                .Where(a => a.ClinicId == clinicId && a.AppointmentDate.Date == date.Date)
                .Select(a => a.AppointmentTime)
                .ToListAsync();

            var allTimes = new List<string>();
            for (int hour = 8; hour <= 17; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    var time = new TimeSpan(hour, minute, 0);
                    var timeString = time.ToString(@"hh\:mm");
                    if (!bookedTimes.Contains(time))
                    {
                        allTimes.Add(timeString);
                    }
                }
            }

            return Json(allTimes);
        }
    }
}
