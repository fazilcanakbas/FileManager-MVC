using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Internet_1.ViewModels
{

    [Table("FileManagerViewModel")]
    public class FileManagerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        [Required]
        public string Type { get; set; } = "DefaultType";  // Varsayılan değer atandı
        public DateTime ModifiedDate { get; set; }
        public long Size { get; set; }
        public string UserId { get; set; }
        public bool? IsFolder { get; set; }  // nullable bool
    }
}