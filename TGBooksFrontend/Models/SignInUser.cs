using TGBooksFrontend.Enums;
using static TGBooksFrontend.Pages.Login;

namespace TGBooksFrontend.Models
{
    public class SignInUser
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public Client client { get; set; }
    }
}
