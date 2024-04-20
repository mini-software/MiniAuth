using Microsoft.AspNetCore.Identity;

namespace MiniAuth.Identity
{
    public class MiniAuthIdentityRole: IdentityRole
    {
        public MiniAuthIdentityRole() : base()
        {
        }
        public MiniAuthIdentityRole(string roleName) : base(roleName)
        {
        }
        public bool Enable { get; set; }=true;
        public string? Type { get; set; }
    }
}
