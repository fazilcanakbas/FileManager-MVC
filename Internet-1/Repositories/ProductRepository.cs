using AutoMapper;
using Internet_1.Models;
using Internet_1.ViewModels;

namespace Internet_1.Repositories
{
    public class ProductRepository : GenericRepository<ViewModels.FileManagerViewModel>
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }
    }
}
