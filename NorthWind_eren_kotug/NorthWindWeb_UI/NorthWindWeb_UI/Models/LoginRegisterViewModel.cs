using Microsoft.AspNetCore.Mvc;

namespace NorthWindWeb_UI.Models
{
    public class LoginRegisterViewModel
	{
		public LoginViewModel? LoginViewModel { get; set; }
		public RegisterViewModel? RegisterViewModel { get; set; }
	}
}
