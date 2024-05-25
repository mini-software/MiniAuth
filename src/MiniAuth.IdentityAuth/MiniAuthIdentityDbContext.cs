using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniAuth.IdentityAuth.Models;


public class MiniAuthIdentityDbContext : IdentityDbContext
{
    public MiniAuthIdentityDbContext(DbContextOptions<MiniAuthIdentityDbContext> options) : base(options)
    {
    }
}

