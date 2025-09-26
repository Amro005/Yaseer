using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using Yaseer.Data;
using Yaseer.Models;

namespace Yaseer.Controllers
{
    public class AccountController : Controller
    {


        private readonly UserManager<User> _userManager;

        private readonly SignInManager<User> _signInManager;

        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;

            _signInManager = signInManager;
            _context = context;

        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            ViewBag.DisabilityTypes = await _context.DisabilityTypes.ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null)
            {
                return Json(new { success = false, message = "طلب غير صالح" });
            }


            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {


                var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
                if (result.Succeeded)
                {
                    var existingClaims = await _signInManager.UserManager.GetClaimsAsync(user);
                    var userNameClaim = existingClaims.FirstOrDefault(c => c.Type == "UserName");

                    if (userNameClaim == null)
                    {
                        var newClaim = new Claim("UserName", user.UserName ?? user.Email);
                        await _signInManager.UserManager.AddClaimAsync(user, newClaim);
                    }
                    else if (userNameClaim.Value != (user.UserName ?? user.Email))
                    {
                        await _signInManager.UserManager.RemoveClaimAsync(user, userNameClaim);
                        var newClaim = new Claim("UserName", user.UserName ?? user.Email);
                        await _signInManager.UserManager.AddClaimAsync(user, newClaim);
                    }




                    return Json(new { success = true, message = "تم تسجيل الدخول بنجاح" });
                }
            }
            return Json(new { success = false, message = "البريد الإلكتروني أو كلمة المرور غير صحيحة" });
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var disabilityTypes = await _context.DisabilityTypes.ToListAsync();
            return View(disabilityTypes);
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null)
            {
                return Json(new { success = false, message = "طلب غير صالح" });
            }
            if (request.Password != request.ConfirmPassword)
            {
                return Json(new { success = false, message = "كلمة المرور وتأكيدها غير متطابقتين" });
            }

            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                FullName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                DisabilityTypeId = request.DisabilityTypeId
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);

                // ⬇️ استدعاء خدمة الإيميل بعد إنشاء الحساب
                var emailService = new EmailService();
                emailService.SendWelcomeEmail(user.Email, user.FullName);

                return Json(new { success = true, message = "تم إنشاء الحساب بنجاح" });
            }

            var error = result.Errors.FirstOrDefault()?.Description ?? "حدث خطأ أثناء إنشاء الحساب";
            return Json(new { success = false, message = error });
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {


            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var appointmentsCount = await _context.Appointments
                    .Where(a => a.UserId == user.Id)
                    .CountAsync();

                ViewBag.AppointmentsCount = appointmentsCount;
            }
            else
            {
                ViewBag.AppointmentsCount = 0;
            }
            return View();
        }
    }
}
