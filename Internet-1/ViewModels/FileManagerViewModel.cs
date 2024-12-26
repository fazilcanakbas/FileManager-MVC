using System.ComponentModel.DataAnnotations;
using Internet_1.Models;

namespace Internet_1.ViewModels
{
    public class FileManagerViewModel : BaseEntity
    {

        public int Id { get; set; } // Primary Key
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public DateTime ModifiedDate { get; set; }
        public long Size { get; set; }
        public string UserId { get; internal set; }
    }
}
