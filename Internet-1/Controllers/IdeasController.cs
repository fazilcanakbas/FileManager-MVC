using Microsoft.AspNetCore.Mvc;
using Internet_1.Models;
using System.Linq;
using Internet_1.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Internet_1.Controllers
{
    //public class IdeasController : Controller
    //{
    //    private readonly ApplicationDbContext _context;

    //    public IdeasController(ApplicationDbContext context)
    //    {
    //        _context = context;

    //    }

   









    // GetIdeaDetails: Fikri ID'ye göre almak
    public class IdeasController : Controller
        {
            private readonly ApplicationDbContext db;
        private readonly ApplicationDbContext _context;
        public IdeasController(ApplicationDbContext context)
            {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        // Fikirlerin listesini al
        public IActionResult Index()
        {
            var ideas = _context.Ideas
                                 .Where(i => !string.IsNullOrEmpty(i.Title) && !string.IsNullOrEmpty(i.Description))
                                 .ToList(); // NULL veya boş olmayan verileri filtrele
            return View(ideas);
        }
        // Fikir Listeleme
        [HttpGet]
            public JsonResult ListAjax()
            {
                var ideas = _context.Ideas.Select(idea => new
                {
                    idea.Id,
                    idea.Title,
                   
                    idea.Category,
                    idea.Likes
                }).ToList();

                return Json(ideas);
            }

            // Fikir Ekleme veya Güncelleme
            [HttpPost]
            public JsonResult AddUpdateAjax(Ideas idea)
            {
                try
                {
                    if (idea.Id == 0)
                    {
                        // Yeni fikir ekleme
                        _context.Ideas.Add(idea);
                    }
                    else
                    {
                        // Mevcut fikir güncelleme
                        var existingIdea = _context.Ideas.Find(idea.Id);
                        if (existingIdea != null)
                        {
                            existingIdea.Title = idea.Title;
                            existingIdea.Description = idea.Description;
                            existingIdea.Category = idea.Category;
                            existingIdea.Likes = idea.Likes;
                        }
                        else
                        {
                            return Json(new { status = false, message = "Fikir bulunamadı!" });
                        }
                    }

                    _context.SaveChanges(); // Değişiklikleri kaydet
                    return Json(new { status = true, message = "Fikir başarıyla kaydedildi!" });
                }
                catch (Exception ex)
                {
                    return Json(new { status = false, message = $"Hata: {ex.Message}" });
                }
            }

            // Fikir Detay Getirme
            [HttpGet]
            public JsonResult GetByIdAjax(int id)
            {
                try
                {
                    var idea = _context.Ideas.Where(i => i.Id == id).Select(i => new
                    {
                        i.Id,
                        i.Title,
                        i.Description,
                        i.Category,
                        i.Likes
                    }).FirstOrDefault();

                    if (idea == null)
                        return Json(new { status = false, message = "Fikir bulunamadı!" });

                    return Json(idea);
                }
                catch (Exception ex)
                {
                    return Json(new { status = false, message = $"Hata: {ex.Message}" });
                }
            }

            // Fikir Silme
            [HttpDelete]
            public JsonResult DeleteAjax(int id)
            {
                try
                {
                    var idea = _context.Ideas.Find(id);
                    if (idea != null)
                    {
                        _context.Ideas.Remove(idea);
                        _context.SaveChanges();
                        return Json(new { status = true, message = "Fikir başarıyla silindi!" });
                    }
                    return Json(new { status = false, message = "Fikir bulunamadı!" });
                }
                catch (Exception ex)
                {
                    return Json(new { status = false, message = $"Hata: {ex.Message}" });
                }
            }
        }
    }