using System.Net;
using System.Net.Mail;

namespace Yaseer.Models
{
    public class EmailService
    {
        public void SendWelcomeEmail(string toEmail, string fullName)
        {
            var fromEmail = "yasseer.welcome@gmail.com";
            var password = "APP_PASSWORD"; // App password من جوجل

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
            };

            var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = "Welcome to Yaseer – Your Comfort Comes First",
                Body = $"Hello {fullName},\n\nThank you for joining Yaseer! You can now easily book medical appointments and request transport.\n\nBest regards,\nThe Yaseer Team",
                IsBodyHtml = false
            };

            smtp.Send(message);
        }
    }
}
