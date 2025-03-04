using System.ComponentModel.DataAnnotations;

namespace BusinessNewEnvironment.Models
{
    public class LoginRequest
    {
        [Key]
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
