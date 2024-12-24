using Microsoft.AspNetCore.Mvc;
using NorthWindWeb_UI.Models;
using System.Diagnostics;
using System.Net.Http;
using static NorthWindWeb_UI.Controllers.EmailServiceController;

namespace NorthWindWeb_UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EmailService _emailService;

        public HomeController(ILogger<HomeController> logger, EmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public IActionResult homepage()
        {
            return View();
        }

        public IActionResult about()
        {
            return View();
        }

        public IActionResult homepage_member()
        {
            return View();
        }

        public IActionResult about_member()
        {
            return View();
        }

        public IActionResult contact_member()
        {
            return View();
        }

        public IActionResult contact()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                string toEmail = "erenkotughelp@gmail.com";
                string subject = model.Subject;
                string body = $"<p><strong>Full Name:</strong> {model.FullName}</p><p><strong>Email:</strong> {model.Email}</p><p><strong>Message:</strong> {model.Message}</p>";
                await _emailService.SendEmailAsync(toEmail, subject, body, model);

                ViewBag.Message = "Email sent successfully!";
            }

            return View("Contact", model);
        }

    }
}
