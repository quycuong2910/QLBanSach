using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QLBanSach.Data.SachRepository;
using QLBanSach.Data.TheLoaiRepository;
using QLBanSach.Models;
using X.PagedList;

namespace QLBanSach.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISacRepository _sacRepository;
        private readonly ITheLoaiRepository _theLoaiRepository;

        public HomeController(ISacRepository sacRepository, ITheLoaiRepository theLoaiRepository)
        {
            _theLoaiRepository = theLoaiRepository;
            _sacRepository = sacRepository;
        }


        public IActionResult Index(string? MaTheLoai, string? TenSach,int page = 1)
        {
            var sach = _sacRepository.GetAll();
            var theloai = _theLoaiRepository.GetAll();
            if (!string.IsNullOrEmpty(MaTheLoai))
            {
                sach = _sacRepository.GetByTl(MaTheLoai);
            }
            if (!string.IsNullOrEmpty(TenSach))
            {
                sach = _sacRepository.GetByName(TenSach);
            }
            int pageSize = 8;
            ViewBag.TheLoai = theloai;
            var pagedSach = sach.ToPagedList(page, pageSize);
            return View(pagedSach);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult ChiTietSach(string masach)
        {
            var sach = _sacRepository.GetById(masach);
            return View(sach);
        }
    }
}
