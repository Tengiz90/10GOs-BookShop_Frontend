namespace TGBooksFrontend.Models
{
    public class EditUserNameResult
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}