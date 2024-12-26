using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Internet_1.Models;
using Internet_1.Repositories;
using Internet_1.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.SignalR;
using Internet_1.Hubs;
using Microsoft.Identity.Client;
using AspNetCoreHero.ToastNotification.Abstractions;

public class FileManagerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly string rootPath;
    private readonly INotyfService _notyf;
    private readonly IWebHostEnvironment _env;

    private readonly IHubContext<GeneralHub> _generalHub;

    public FileManagerController(ApplicationDbContext context, INotyfService notyf, IWebHostEnvironment env, IHubContext<GeneralHub> generalHub)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _env = env;
        _generalHub = generalHub;
        _notyf = notyf;   

    }

    // Index metodu: Dosya veya klasörleri listelemek için kullanılır
    public IActionResult Index(string folderPath = "")
    {
        // Eğer folderPath boşsa tüm dosya ve klasörleri getir
        var filesAndFolders = string.IsNullOrEmpty(folderPath)
      ? _context.FileManagerViewModel.ToList()
      : _context.FileManagerViewModel
          .Where(f => (f.Path ?? "").StartsWith(folderPath.TrimStart('/'))) // 'folderPath' başındaki '/' kaldırarak karşılaştırma yapıyoruz
          .ToList();


        ViewBag.CurrentPath = folderPath;

        return View("~/Views/Home/Index.cshtml", filesAndFolders);
    }





    // UploadFile metodu: Dosya yüklemek için kullanılır
    [HttpPost]
    public async Task<IActionResult> UploadFile(string folderPath, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["UploadMessage"] = "Dosya Seçili Değil.";
            return RedirectToAction("Index", new { folderPath });
        }

        // Hedef dizini belirle
        var targetFolder = Path.Combine(rootPath, folderPath ?? "").TrimEnd(Path.DirectorySeparatorChar);

        // Hedef dizin kontrolü
        if (!targetFolder.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            TempData["UploadMessage"] = "Dosya Yolu Bulunamadı";
            return RedirectToAction("Index", new { folderPath });
        }

        // Hedef dizin yoksa oluştur
        if (!Directory.Exists(targetFolder))
        {
            try
            {
                Directory.CreateDirectory(targetFolder);
            }
            catch (Exception ex)
            {
                TempData["UploadMessage"] = $"Error creating directory: {ex.Message}";
                return RedirectToAction("Index", new { folderPath });
            }
        }

        // Dosyanın tam yolu
        var filePath = Path.Combine(targetFolder, file.FileName);

        // Dosya modeli oluştur
        var fileManagerViewModel = new FileManagerViewModel
        {
            Name = file.FileName,
            Path = filePath,
            Size = file.Length,
            ModifiedDate = DateTime.Now,
            Type = "File",
            UserId = "SomeUserId" // Gerekirse kullanıcı bilgisi ekleyin

        };

      

        try
        {
            // Dosyayı sisteme kaydet
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Veritabanına kaydet
            _context.FileManagerViewModel.Add(fileManagerViewModel);
            await _context.SaveChangesAsync();

            TempData["UploadMessage"] = "Dosya Başarıyla Yüklendi";
        }
        catch (Exception ex)
        {
            TempData["UploadMessage"] = $"Error occurred: {ex.Message}";
        }
        // FileManagerViewModel içindeki türü 'file' olan kayıtları say
        int filecatCount = _context.FileManagerViewModel
            .Where(f => f.Type == "file") // Burada Type alanını 'file' olanlarla filtreliyoruz
            .Count();

        // SignalR üzerinden tüm istemcilere bildir
        await _generalHub.Clients.All.SendAsync("onFileAdd", filecatCount);



        return RedirectToAction("Index", new { folderPath });
    }

    // Delete metodu: Dosya veya klasör silmek için kullanılır
    public async Task<ActionResult> Delete(string path, string type)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(type))
        {
            TempData["DeleteMessage"] = "Hata: Geçersiz yol veya tür.";
            return BadRequest("Path or type cannot be empty.");
        }

        var decodedPath = WebUtility.UrlDecode(path);
        var fullPath = Path.Combine(rootPath, decodedPath);

        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            TempData["DeleteMessage"] = "Hata: Bu dosyaya erişim izni yok.";
            return Forbid("Access to the specified file is not allowed.");
        }

        if (type == "Folder" && !Directory.Exists(fullPath))
        {
            TempData["DeleteMessage"] = $"Hata: Klasör bulunamadı: {fullPath}";
            return NotFound($"Folder not found: {fullPath}");
        }

        if (type == "File" && !System.IO.File.Exists(fullPath))
        {
            TempData["DeleteMessage"] = $"Hata: Dosya bulunamadı: {fullPath}";
      
        }

        try
        {
            // Dosya veya klasör silme işlemi
            if (type == "DefaultType")
            {
                Directory.Delete(fullPath, true);

            }
            else
            {
                System.IO.File.Delete(fullPath);
            }

            // Veritabanındaki kaydı sil
            var fileManagerViewModel = await _context.FileManagerViewModel
                .FirstOrDefaultAsync(f => f.Path == fullPath);

            int filecatCount = _context.FileManagerViewModel
              .Where(f => f.Type == "file") // Burada Type alanını 'file' olanlarla filtreliyoruz
              .Count();

            // SignalR üzerinden tüm istemcilere bildir
            await _generalHub.Clients.All.SendAsync("onFileDelete", filecatCount);
           
            if (fileManagerViewModel != null)
            {
                _context.FileManagerViewModel.Remove(fileManagerViewModel);
                await _context.SaveChangesAsync();
            }
            // FileManagerViewModel içindeki türü 'file' olan kayıtları say
          
            TempData["DeleteMessage"] = type == "Folder" ? "Klasör başarıyla silindi." : "Dosya başarıyla silindi.";
            return RedirectToAction("Index" ,"FileManager");
        }

        catch (Exception ex)
        {
            TempData["DeleteMessage"] = $"Hata: Silme işlemi sırasında bir hata oluştu: {ex.Message}";
            return BadRequest($"An error occurred while deleting: {ex.Message}");
        }

      
    }

    public async Task<ActionResult> Download(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            TempData["DownloadMessage"] = "Hata: Dosya yolu boş olamaz.";
            return BadRequest("File path cannot be empty.");
        }

        var decodedFilePath = WebUtility.UrlDecode(filePath);
        var fullPath = Path.Combine(rootPath, decodedFilePath.TrimStart(Path.DirectorySeparatorChar));

        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            TempData["DownloadMessage"] = "Hata: Bu dosyaya erişim izni yok.";
            return Forbid("Access to the specified file is not allowed.");
        }

        if (!System.IO.File.Exists(fullPath))
        {
            TempData["DownloadMessage"] = $"Hata: Belirtilen dosya mevcut değil: {fullPath}";
            return NotFound($"The specified file does not exist: {fullPath}");
        }
        // FileManagerViewModel içindeki türü 'file' olan kayıtları say
        int filecatCount = _context.FileManagerViewModel
            .Where(f => f.Type == "file") // Burada Type alanını 'file' olanlarla filtreliyoruz
            .Count();

        // SignalR üzerinden tüm istemcilere bildir
        await _generalHub.Clients.All.SendAsync("onFileDowloand", filecatCount);
        try
        {
            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            var fileName = Path.GetFileName(fullPath);

            // Başarılı mesaj
            TempData["DownloadMessage"] = $"{fileName} başarıyla indirildi.";
            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            TempData["DownloadMessage"] = $"Hata: Dosya indirilirken bir hata oluştu: {ex.Message}";
            return BadRequest($"An error occurred while processing the file: {ex.Message}");
        }
    }

    public IActionResult GetImage(string imageName)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", imageName);
        if (System.IO.File.Exists(filePath))
        {
            var fileStream = System.IO.File.OpenRead(filePath);
            return File(fileStream, "image/jpeg");
        }
        return NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFolderAsync(string folderName)
    {
        try
        {
            // Klasör yolunu fiziksel olarak oluşturma yerine sadece veritabanına kaydediyoruz.
            string virtualFolderPath = folderName; // Sanal yol

            // Veritabanına kaydetme işlemi
            var folder = new FileManagerViewModel
            {
                Name = folderName,
                Path = virtualFolderPath, // Sadece sanal yol veriyoruz
                IsFolder = true,
                ModifiedDate = DateTime.Now,
                Size = 0,
                UserId = "1" // UserId'yi düzgün şekilde atadığınızdan emin olun
            };

            // Veritabanına ekleme işlemi
            _context.FileManagerViewModel.Add(folder);
            

            try
            {
                _context.SaveChanges(); // Veritabanına kaydet
                TempData["UploadMessage"] = "Klasör Oluşturuldu";
                int foldercatCount = _context.FileManagerViewModel
                  .Where(f => f.Type == "DefaultType") // Burada Type alanını 'file' olanlarla filtreliyoruz
                  .Count();

                // SignalR üzerinden tüm istemcilere bildir
                await _generalHub.Clients.All.SendAsync("onFolderAdd", foldercatCount);
            }
            catch (Exception saveEx)
            {
                TempData["UploadMessage"] = "Klasör veritabanına kaydedilirken bir hata oluştu: " + saveEx.Message;
                // InnerException'ı logluyoruz
                Console.WriteLine("Inner Exception: " + saveEx.InnerException?.Message);
                TempData["UploadMessage"] += "\n" + "Inner Exception: " + saveEx.InnerException?.Message;
            }
        }
        catch (Exception ex)
        {
            TempData["UploadMessage"] = "Klasör oluşturulurken bir hata oluştu: " + ex.Message;
            // Inner Exception'ı da logluyoruz
            Console.WriteLine("Inner Exception: " + ex.InnerException?.Message);
            TempData["UploadMessage"] += "\n" + "Inner Exception: " + ex.InnerException?.Message;
        }
       

        return RedirectToAction("Index");
    }
    [HttpGet("DeleteFolder")]
    public async Task<ActionResult> DeleteFolder(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return Json(new { success = false, message = "Geçersiz yol" });
        }

        try
        {
            // Gelen path'i çözümle
            var decodedPath = WebUtility.UrlDecode(path);

            // Veritabanında bu klasörü bul
            var folder = await _context.FileManagerViewModel
                .FirstOrDefaultAsync(f => f.Path == decodedPath && f.IsFolder == true);

            if (folder == null)
            {
                return Json(new { success = false, message = "Klasör bulunamadı." });
            }

            // Klasörü ve içindeki ilgili öğeleri (alt klasörler veya dosyalar) kaldır
            var relatedItems = _context.FileManagerViewModel
                .Where(f => f.Path.StartsWith(decodedPath));

            _context.FileManagerViewModel.RemoveRange(relatedItems); // İlgili tüm öğeleri kaldır
            await _context.SaveChangesAsync();

            // SignalR ile tüm istemcilere bildir
            int folderCount = _context.FileManagerViewModel.Count(f => f.IsFolder == true);
            await _generalHub.Clients.All.SendAsync("onFolderDelete", folderCount);

            TempData["DeleteMessage"] = "Klasör başarıyla silindi." ;
            return RedirectToAction("Index", "FileManager");
        }
        catch (Exception ex)
        {
            TempData["DownloadMessage"] = $"Hata:Klasör Silinirken bir hata oluştu: {ex.Message}";
            return BadRequest($"An error occurred while processing the file: {ex.Message}");
        }
    }

}