using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Yaseer.Models;

namespace Yaseer.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<DisabilityType> DisabilityTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

           
            builder.Entity<Clinic>().HasData(
                new Clinic
                {
                    Id = 1,
                    Name = "عيادة الأمل للعلاج الطبيعي",
                    Description = "عيادة متخصصة في العلاج الطبيعي لذوي الاحتياجات الخاصة",
                    Address = "شارع الملك فهد، الرياض",
                    Specialization = "العلاج الطبيعي والإعاقة الحركية",
                    PhoneNumber = "0112345678",
                    ImageUrl = "~/images/Home_1.jpg"
                },
                new Clinic
                {
                    Id = 2,
                    Name = "مركز النور للسمعيات",
                    Description = "مركز متخصص في علاج مشاكل السمع والنطق",
                    Address = "شارع العليا، الرياض",
                    Specialization = "السمعيات والنطق",
                    PhoneNumber = "0112345679",
                    ImageUrl = "~/images/Home_1.jpg"
                },
                new Clinic
                {
                    Id = 3,
                    Name = "عيادة المستقبل للبصريات",
                    Description = "عيادة متخصصة في علاج مشاكل البصر",
                    Address = "شارع التحلية، جدة",
                    Specialization = "البصريات والإعاقة البصرية",
                    PhoneNumber = "0212345678",
                    ImageUrl = "~/images/Home_1.jpg"
                }
            );
        }
    }
}

