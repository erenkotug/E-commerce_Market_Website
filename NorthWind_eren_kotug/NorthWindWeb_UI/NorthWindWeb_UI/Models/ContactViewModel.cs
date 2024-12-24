using Microsoft.AspNetCore.Mvc;

namespace NorthWindWeb_UI.Models
{
    public class ContactViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}
