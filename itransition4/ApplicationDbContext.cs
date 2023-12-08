using itransition4.Models;
using Microsoft.EntityFrameworkCore;

namespace itransition4
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Users> Users { get; set;}
    }
}

