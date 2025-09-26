using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yaseer.Data;
using Yaseer.Models;

namespace Yaseer.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var totalUsers = await _userManager.Users.CountAsync();
            var totalAppointments = await _context.Appointments.CountAsync();
            var totalClinics = await _context.Clinics.CountAsync();
            var recentAppointments = await _context.Appointments
                .Include(a => a.Clinic)
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();

            var clinics = await _context.Clinics.ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.TotalClinics = totalClinics;
            ViewBag.RecentAppointments = recentAppointments;
            ViewBag.Clinics = clinics;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return Json(new { success = false, message = "الموعد غير موجود" });
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "تم حذف الموعد بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء حذف الموعد" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClinic(int id)
        {
            try
            {
                var clinic = await _context.Clinics.FindAsync(id);
                if (clinic == null)
                {
                    return Json(new { success = false, message = "العيادة غير موجودة" });
                }

                var hasAppointments = await _context.Appointments.AnyAsync(a => a.ClinicId == id);
                if (hasAppointments)
                {
                    return Json(new { success = false, message = "لا يمكن حذف العيادة لوجود مواعيد مرتبطة بها" });
                }

                _context.Clinics.Remove(clinic);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "تم حذف العيادة بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء حذف العيادة" });
            }
        }
    }
}
