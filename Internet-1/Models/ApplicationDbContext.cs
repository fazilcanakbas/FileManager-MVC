using Microsoft.EntityFrameworkCore;

namespace Internet_1.Models
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Note> Notes { get; set; } // Notes tablosu
        public DbSet<FileManagerModel> FileManagerModel { get; set; }
        public object Files { get; internal set; }
        public object Folders { get; internal set; }




    }
}
