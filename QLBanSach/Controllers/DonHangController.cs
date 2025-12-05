using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using QLBanSach.Data.ChiTietDonHangRepository;
using QLBanSach.Data.ChiTietGioHangRepository;
using QLBanSach.Data.DonHangRepository;
using QLBanSach.Data.GioHangRepository;
using QLBanSach.Data.NguoiDungRepository;
using QLBanSach.Data.SachRepository;
using QLBanSach.Models;
using X.PagedList;

namespace QLBanSach.Controllers
{
    public class DonHangController : Controller
    {
        private readonly IGioHangRepository _gioHangRepository;
        private readonly IChiTietGioHangRepository _chiTietGioHangRepository;
        private readonly IDonHangRepository _donHangRepository;
        private readonly IChiTietDonHangRepository _chiTietDonHangRepository;
        private readonly ISachRepository _sacRepository;
        private readonly INguoiDungRepository _nguoiDungRepository;

        public DonHangController(IGioHangRepository gioHangRepository, IChiTietGioHangRepository chiTietGioHangRepository, IDonHangRepository donHangRepository, IChiTietDonHangRepository chiTietDonHangRepository, ISachRepository sacRepository, INguoiDungRepository guoiDungRepository)
        {
            _gioHangRepository = gioHangRepository;
            _chiTietGioHangRepository = chiTietGioHangRepository;
            _donHangRepository = donHangRepository;
            _chiTietDonHangRepository = chiTietDonHangRepository;
            _sacRepository = sacRepository;
            _nguoiDungRepository = guoiDungRepository;
        }

        private string GenerateMaDonHang()
        {
            var lastOrder = _donHangRepository.GetAll()
                .OrderByDescending(d => d.MaDonHang)
                .FirstOrDefault();
            if (lastOrder == null)
                return "DH00001";
            string lastCode = lastOrder.MaDonHang.Replace("DH", "");
            int number = int.Parse(lastCode) + 1;
            return "DH" + number.ToString("D5");
        }
        private string GenerateMaChiTietDonHang()
        {
            var lastDetail = _chiTietDonHangRepository.GetAll()
                .OrderByDescending(c => c.MaChiTiet)
                .FirstOrDefault();

            if (lastDetail == null)
                return "CTDH000001";

            string lastCode = lastDetail.MaChiTiet.Replace("CTDH", "");

            int number = int.Parse(lastCode) + 1;

            return "CTDH" + number.ToString("D6");
        }
        public IActionResult Index(string id, string trangThai, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = HttpContext.Session.GetString("UserId");
            }

            var query = _donHangRepository.GetIDNguoiDung(id).AsQueryable();

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            // Sắp xếp
            query = query.OrderByDescending(x => x.NgayDat);

            // Phân trang
            var pagedList = query.ToPagedList(page, pageSize);

            // Giữ giá trị lọc để truyền lại vào View
            ViewBag.TrangThai = trangThai;
            ViewBag.IdNguoiDung = id;

            return View(pagedList);
        }

        public string GenerateMaCTDH()
        {
            string? lastCode = _chiTietDonHangRepository.GetAll().OrderByDescending(x => x.MaChiTiet)
                .Select(x => x.MaChiTiet)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(lastCode))
            {
                return "CTDH000001";
            }
            int number = int.Parse(lastCode.Substring(4));
            number++;
            return "CTDH" + number.ToString("D6");
        }
        [HttpGet]
        public IActionResult DatHang(string maGioHang)
        {
            var GioHang = _gioHangRepository.GetById(maGioHang);
            var NguoiDung = _nguoiDungRepository.GetById(GioHang.MaNguoiDung);
            var ChiTietGioHang = _chiTietGioHangRepository.GetByIDGioHang(maGioHang); 
            var DonHang = new DonHang
            {
                MaDonHang = GenerateMaDonHang(),
                MaNguoiDung = NguoiDung.MaNguoiDung,
                TrangThai = "Đã đặt hàng",
                NgayDat = DateTime.Now,
                DiaChiNhanHang = NguoiDung.DiaChi,
                TongTien = _chiTietGioHangRepository.GetPrice(maGioHang),
            };
            ViewBag.MaGioHang = maGioHang;
            return View(DonHang);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DatHang(DonHang donHang ,string maGioHang, bool PhuongThucThanhToan)
        {
            if(PhuongThucThanhToan)
            {
                Console.WriteLine("Z");
            }
            _donHangRepository.Add(donHang);
            _donHangRepository.Save();
            var ChiTietGioHang = _chiTietGioHangRepository.GetByIDGioHang(maGioHang);
            foreach (var item in ChiTietGioHang)
                {
                    _chiTietDonHangRepository.Add(new ChiTietDonHang
                    {
                        MaChiTiet = GenerateMaCTDH(),
                        MaDonHang = donHang.MaDonHang,
                        MaSach = item.MaSach,
                        SoLuong = item.SoLuong,
                    });
                    _chiTietDonHangRepository.Save();
                    _chiTietGioHangRepository.Delete(item);
                    _chiTietGioHangRepository.Save();
                }
                _gioHangRepository.Delete(_gioHangRepository.GetById(maGioHang));
                _gioHangRepository.Save();
                TempData["Message"] = "Đã đặt hàng thành công";
                TempData["MesseType"] = "success";
                return RedirectToAction("Index", "Home");
        }
        public IActionResult Detail(string MaDonHang)
        {
            var Donhang = _donHangRepository.GetCTDH(MaDonHang);
            return View(Donhang);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HuyHang(string MaDonHang)
        {
            var donhang = _donHangRepository.GetById(MaDonHang);
            donhang.TrangThai = "Đã hủy";
            _donHangRepository.Update(donhang);
            _donHangRepository.Save();


            TempData["Message"] = $"Đơn hàng {donhang.MaDonHang} đã được hủy!";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NhanHang(string MaDonHang)
        {
            var donhang = _donHangRepository.GetById(MaDonHang);

            donhang.TrangThai = "Đã nhận hàng";
            _donHangRepository.Update(donhang);
            _donHangRepository.Save();

            TempData["Message"] = $"Bạn đã nhận đơn hàng {donhang.MaDonHang}!";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index", "Home");
        }

    }
}
