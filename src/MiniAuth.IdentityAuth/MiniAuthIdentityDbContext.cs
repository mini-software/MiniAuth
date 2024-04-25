using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniAuth.IdentityAuth.Models;

namespace MiniAuth.Identity
{
    public class MiniAuthIdentityDbContext : IdentityDbContext
    {
        public MiniAuthIdentityDbContext(DbContextOptions<MiniAuthIdentityDbContext> options) : base(options)
        {
        }
    }
}
