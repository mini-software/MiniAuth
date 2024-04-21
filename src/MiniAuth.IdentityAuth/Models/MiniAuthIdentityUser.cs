using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniAuth.IdentityAuth.Models
{
    [Table("AspNetUsers")]
    public class MiniAuthIdentityUser : IdentityUser
    {
        public string? First_name { get; set; }
        public string? Last_name { get; set; }
        public string? Emp_no { get; set; }
        public bool Enable { get; set; } = true;
        public string? Type { get; set; }
    }

    public class MiniAuthUserVo
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public string Emp_no { get; set; }
        public string Mail { get; set; }
        public bool Enable { get; set; }
        public string[] Roles { get; set; }
        public string Type { get; set; }
    }
}
