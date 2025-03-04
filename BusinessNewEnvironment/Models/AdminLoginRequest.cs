using System.ComponentModel.DataAnnotations;

namespace BusinessNewEnvironment.Models
{
    public class AdminLoginRequest
    {
        [Key]
        public int Id { get; set; }
        public string EmailId { get; set; }
        public string AdminPassword { get; set; }
        public int RoleId { get; set; }
        public bool IsPasswordChanged { get; set; }
    }
}
