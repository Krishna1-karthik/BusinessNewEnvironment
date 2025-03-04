using System.ComponentModel.DataAnnotations;

namespace BusinessNewEnvironment.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
