using Microsoft.EntityFrameworkCore;
using Internet_1.Models;
using Internet_1.ViewModels;

namespace Internet_1.Repositories
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
        public DbSet<FileManagerViewModel> FileManagerViewModel { get; set; }
        public object Files { get; internal set; }
        public object Folders { get; internal set; }

        internal static object Where(Func<object, bool> value)
        {
            throw new NotImplementedException();
        }

        internal static int Count()
        {
            throw new NotImplementedException();
        }
    }
}
