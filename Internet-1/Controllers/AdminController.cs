using Internet_1.Repositories;
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
        var folders = _context.FileManagerViewModel
            .Where(f => f.Type == "DefaultType")
            .ToList();
        return View(folders);
    }

    // Dosyalar sayfası
    public IActionResult FilesIndex()
    {
        var files = _context.FileManagerViewModel
            .Where(f => f.Type == "file")
            .ToList();
        return View(files);
    }

    // Yüklenen dosyaların listelenmesi (dosyalar ve klasörler türüne göre)
    public IActionResult Index()
    {
        // Dosya ve Klasör Sayılarını Al
        var fileCount = _context.FileManagerViewModel.Count(f => f.IsFolder == null); // Dosya sayısını al
        var folderCount = _context.FileManagerViewModel.Count(f => f.IsFolder == true); // Klasör sayısını al

        // Sayfaya gönder
        ViewBag.FileCount = fileCount;
        ViewBag.FolderCount = folderCount;

        return View();
        // Admin/Index.cshtml view'ını döner
       
    }


    // Dosyanın silinmesi
    public IActionResult Delete(int id, string returnUrl)
    {
        var fileRecord = _context.FileManagerViewModel.FirstOrDefault(f => f.Id == id);
        if (fileRecord == null)
            return NotFound();

        try
        {
            // Dosyayı sistemden sil
            if (System.IO.File.Exists(fileRecord.Path))
            {
                System.IO.File.Delete(fileRecord.Path);
            }

            // Veritabanından sil
            _context.FileManagerViewModel.Remove(fileRecord);
            _context.SaveChanges();
        }
        catch
        {
            return BadRequest("Dosya silme işlemi başarısız oldu.");
        }

        // returnUrl parametresi varsa, ona göre yönlendirme yap
        if (string.IsNullOrEmpty(returnUrl))
        {
            return RedirectToAction("FilesIndex"); // Varsayılan sayfa
        }

        return Redirect(returnUrl); // returnUrl'yi kullanarak yönlendirme yap
    }
    public IActionResult Download(int id)
    {
        var fileRecord = _context.FileManagerViewModel.FirstOrDefault(f => f.Id == id);
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



    public IActionResult DeleteFolder(int id, string returnUrl)
    {
        var folder = _context.FileManagerViewModel.FirstOrDefault(f => f.Id == id && f.Type == "DefaultType");
        if (folder == null)
            return NotFound();

        try
        {
            // Klasörü silme işlemi
            if (Directory.Exists(folder.Path))
            {
                Directory.Delete(folder.Path, true); // true ile içindeki tüm dosyalar da silinir
            }

            // Veritabanından klasörü sil
            _context.FileManagerViewModel.Remove(folder);
            _context.SaveChanges();
        }
        catch
        {
            return BadRequest("Klasör silme işlemi başarısız oldu.");
        }

        // returnUrl parametresine göre yönlendirme
        if (string.IsNullOrEmpty(returnUrl))
        {
            return RedirectToAction("FoldersIndex");
        }

        return Redirect(returnUrl);
    }



}