using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniAuth.IdentityAuth.Models
{
    [Table("AspNetUsers")]
    public class MiniAuthIdentityUser : IdentityUser
    {
    }
}
