using BookApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BookApi.Dal
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Book> Books { get; set; }
        //
    }
}
