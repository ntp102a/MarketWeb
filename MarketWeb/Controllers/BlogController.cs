using AspNetCoreHero.ToastNotification.Abstractions;
using MarketWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;

namespace MarketWeb.Controllers
{
    public class BlogController : Controller
    {
        private readonly dbMarketsContext _context;
        public BlogController(dbMarketsContext context)
        {
            _context = context;
        }
        [Route("blogs.html", Name = "Blog")]
        public IActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var IsPages = _context.TinDangs
                .AsNoTracking()
                .OrderByDescending(x => x.PostId);
            PagedList<TinDang> models = new PagedList<TinDang>(IsPages, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        [Route("/tin-tuc/{Alias}-{id}.html", Name ="TinDetails")]
        public IActionResult Details(int id)
        {
            var tindang = _context.TinDangs.AsNoTracking().SingleOrDefault(x => x.PostId == id);
            if (tindang == null)
            {
                return RedirectToAction("Index");
            }
            var lsBaivietlienquan = _context.TinDangs
                .AsNoTracking()
                .Where(x => x.PostId != id && x.Published == true)
                .Take(3).OrderByDescending(x => x.CreatedDate)
                .ToList();
            ViewBag.Baivietlienquan = lsBaivietlienquan;
            return View(tindang);
        }
    }
}
