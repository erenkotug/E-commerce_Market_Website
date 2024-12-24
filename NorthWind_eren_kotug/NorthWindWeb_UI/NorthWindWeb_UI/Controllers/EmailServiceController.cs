using Microsoft.AspNetCore.Mvc;
using NorthWindWeb_UI.Models;
using System.Net.Mail;
using System.Net;
using static NorthWindWeb_UI.Controllers.HomeController;

namespace NorthWindWeb_UI.Controllers
{
    public class EmailServiceController : Controller
    {
        private readonly EmailService _emailService;

        public EmailServiceController(EmailService emailService)
        {
            _emailService = emailService;
        }

        public class EmailService
        {
            public async Task SendEmailAsync(string toEmail, string subject, string body, ContactViewModel model)
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("erenkotughelp@gmail.com", "fwlrfozzulwcqixk"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(model.Email),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

        }
    }
}
