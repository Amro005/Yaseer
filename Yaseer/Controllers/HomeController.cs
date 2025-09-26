using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yaseer.Data;
using Yaseer.Models;

namespace Yaseer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var clinics = await _context.Clinics.ToListAsync();
            return View(clinics);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

