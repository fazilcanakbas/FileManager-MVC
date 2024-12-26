using Internet_1.Models;
using Internet_1.ViewModels;
using Microsoft.AspNetCore.Mvc;

public class AdminController : Controller
{
    private readonly string _uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    private readonly ApplicationDbContext _context;   

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult FoldersIndex()
    {
        var folders = _context.FileManagerModel
            .Where(f => f.Type == "DefaultType")
            .ToList();
        return View(folders);
    }

    // Dosyalar sayfası
    public IActionResult FilesIndex()
    {
        var files = _context.FileManagerModel
            .Where(f => f.Type == "file")
            .ToList();
        return View(files);
    }

    // Yüklenen dosyaların listelenmesi (dosyalar ve klasörler türüne göre)
    public IActionResult Index()
    {
        // Dosya ve Klasör Sayılarını Al
        var fileCount = _context.FileManagerModel.Count(f => f.IsFolder == null); // Dosya sayısını al
        var folderCount = _context.FileManagerModel.Count(f => f.IsFolder == true); // Klasör sayısını al

        // Sayfaya gönder
        ViewBag.FileCount = fileCount;
        ViewBag.FolderCount = folderCount;

        return View();
        // Admin/Index.cshtml view'ını döner
       
    }


    // Dosyanın silinmesi
    // Dosyanın veya klasörün silinmesi
    public IActionResult Delete(int id)
    {
        var fileRecord = _context.FileManagerModel.FirstOrDefault(f => f.Id == id);
        if (fileRecord == null)
            return NotFound();

        try
        {
            // Eğer dosya değilse (yani klasörse), klasörün fiziksel olarak var olup olmadığını kontrol edin
            if (fileRecord.IsFolder == true)
            {
                if (Directory.Exists(fileRecord.Path))
                {
                    Directory.Delete(fileRecord.Path, true); // Klasörü ve içindekileri sil
                }
            }
            else
            {
                // Dosyayı sistemden sil
                if (System.IO.File.Exists(fileRecord.Path))
                {
                    System.IO.File.Delete(fileRecord.Path);
                }
            }

            // Veritabanından sil
            _context.FileManagerModel.Remove(fileRecord);
            _context.SaveChanges();
        }
        catch
        {
            return BadRequest("Silme işlemi başarısız oldu.");
        }

        // Kullanıcıyı doğru sayfaya yönlendirin
        if (fileRecord.IsFolder == true)
        {
            return RedirectToAction("FoldersIndex"); // Klasör silinince buraya yönlendirin
        }
        else
        {
            return RedirectToAction("FilesIndex"); // Dosya silinince buraya yönlendirin
        }
    }

    public IActionResult Download(int id)
    {
        var fileRecord = _context.FileManagerModel.FirstOrDefault(f => f.Id == id);
        if (fileRecord == null)
            return NotFound();

        // Dosya yolunu veritabanından alıyoruz
        var filePath = fileRecord.Path;

        if (System.IO.File.Exists(filePath))
        {
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", Path.GetFileName(filePath));
        }

        return NotFound(); // Dosya bulunamadı
    }



   


}