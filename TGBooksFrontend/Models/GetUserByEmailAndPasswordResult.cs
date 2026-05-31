namespace TGBooksFrontend.Models
{
    public class GetUserByEmailAndPasswordResult
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string JwtToken { get; set; } = "";
        public bool IsVerified { get; set; }
        public object Role { get; set; } = "";
    }
}
