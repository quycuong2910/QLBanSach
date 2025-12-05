using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using QLBanSach.Data.DonHangRepository;
using QLBanSach.Data.GioHangRepository;
using QLBanSach.Data.NguoiDungRepository;
using QLBanSach.Models;
using System.Net.Mail;
using System.Net;
using System.Text.Json;

namespace QLBanSach.Controllers
{
    public class AccountController : Controller
    {
        private readonly INguoiDungRepository _nguoiDungRepository;
        private readonly IDonHangRepository _donHangRepository;

        public AccountController(INguoiDungRepository nguoiDungRepository, IDonHangRepository donHangRepository)
        {
            _nguoiDungRepository = nguoiDungRepository;
            _donHangRepository = donHangRepository;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            var Check = _nguoiDungRepository.LogIn(username, password);

            if (Check)
            {
                var nguoiDung = _nguoiDungRepository.GetByName(username);
                HttpContext.Session.SetString("UserName", nguoiDung.HoTen);
                HttpContext.Session.SetString("UserId",nguoiDung.MaNguoiDung);
                HttpContext.Session.SetString("VaiTro", nguoiDung.VaiTro.ToString());
                TempData["Message"] = "Xin chào "+nguoiDung.HoTen;
                TempData["MesseType"] = "success";
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Message"] = "Bạn đã đăng xuất";
            TempData["MesseType"] = "error";
            return RedirectToAction("Index", "Home");
        }

        private string GenerateMaNguoiDung()
        {
            var lastUser = _nguoiDungRepository.GetAll()
            .Where(u => u.MaNguoiDung != null && u.MaNguoiDung.StartsWith("ND"))
            .Select(u =>
            {
                int num;
                return int.TryParse(u.MaNguoiDung.Substring(2), out num) ? num : 0;
            })
            .OrderByDescending(num => num)
            .FirstOrDefault();
            int nextNumber = lastUser + 1;
            return $"ND{nextNumber:00}";
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NguoiDung user)
        {
            var existingUser = _nguoiDungRepository.GetAll().FirstOrDefault(u =>
                u.TaiKhoan == user.TaiKhoan);

            if (existingUser != null)
            {
                TempData["Message"] = "Tài khoản trùng ";
                TempData["MessageType"] = "warning";
                return View(user);
            }
            if (_nguoiDungRepository.EmailExists(user.Email))
            {
                TempData["Message"] = "Email bị trùng ";
                TempData["MessageType"] = "warning";
                return View(user);
            }
            string otp = new Random().Next(100000, 999999).ToString();

            var pendingUser = new PendingUser
            {
                TaiKhoan = user.TaiKhoan,
                MatKhau = user.MatKhau,
                HoTen = user.HoTen,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai,
                DiaChi = user.DiaChi,
                VaiTro = user.VaiTro,
                OTP = otp,
                OTPExpire = DateTime.Now.AddMinutes(5)
            };
            HttpContext.Session.SetString("PendingUser", JsonSerializer.Serialize(pendingUser));

            SendEmailCode(pendingUser.Email, pendingUser.OTP);

            return RedirectToAction("EmailConfirm");
        }
        private void SendEmailCode(string email,string otp)
        {
            var sender = "webbansach2004@gmail.com";
            var appPassword = "iiee yeon rpet piht";
            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(sender, appPassword),
                EnableSsl = true,
            };
            smtp.Send(sender, email, "Mã xác nhận từ webbansach.com", $"Mã xác nhận của bạn là: {otp} (Hết hạn sau 5 phút)");
        }

        [HttpGet]
        public IActionResult EmailConfirm()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EmailConfirm(string code)
        {
            var pendingJson = HttpContext.Session.GetString("PendingUser");
            if(pendingJson == null)
            {
                TempData["Message"] = "Không tìm thấy thông tin đăng ký!";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Create");
            }
            var pendingUser = JsonSerializer.Deserialize<PendingUser>(pendingJson);
            if(pendingUser.OTPExpire < DateTime.Now)
            {
                TempData["Message"] = "Mã OTP đã hết hạn!";
                TempData["MessageType"] = "warning";
                return View();
            }
            if(pendingUser.OTP == code)
            {
                var user = new NguoiDung
                {
                    MaNguoiDung = GenerateMaNguoiDung(),
                    TaiKhoan = pendingUser.TaiKhoan,
                    MatKhau = pendingUser.MatKhau,
                    HoTen = pendingUser.HoTen,
                    Email = pendingUser.Email,
                    SoDienThoai = pendingUser.SoDienThoai,
                    DiaChi = pendingUser.DiaChi,
                    VaiTro = pendingUser.VaiTro
                };
                _nguoiDungRepository.Add(user);
                _nguoiDungRepository.Save();
                HttpContext.Session.Remove("PendingUser");
                TempData["Message"] = "Xác nhận email thành công! Bạn có thể đăng nhập.";
                TempData["MessageType"] = "success";
                return RedirectToAction("Login");
            }
            else
            {
                TempData["Message"] = "Mã OTP không đúng!";
                TempData["MessageType"] = "warning";
                return View();
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResendOTP()
        {
            var pendingJson = HttpContext.Session.GetString("PendingUser");
            if (pendingJson == null)
            {
                TempData["Message"] = "Không tìm thấy thông tin đăng ký!";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Create");
            }
            var pendingUser = JsonSerializer.Deserialize<PendingUser>(pendingJson);
            string otp = new Random().Next(100000, 999999).ToString();
            pendingUser.OTP = otp;
            pendingUser.OTPExpire = DateTime.Now.AddMinutes(5);
            HttpContext.Session.SetString("PendingUser", JsonSerializer.Serialize(pendingUser));
            SendEmailCode(pendingUser.Email, pendingUser.OTP);
            TempData["Message"] = "Vui lòng kiểm tra email để nhận mã xác nhận!";
            TempData["MessageType"] = "success";
            return RedirectToAction("EmailConfirm");
        }

        public IActionResult Details(string id)
        {
            var nguoidung = _nguoiDungRepository.GetById(id);
            return View(nguoidung);
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Message"] = "Vui lòng kiểm tra email để nhận mật khẩu mới!";
                TempData["MessageType"] = "warning";
                return View();
            }

            var user = _nguoiDungRepository.GetAll()
                .FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                TempData["Message"] = "Vui lòng kiểm tra email không tồn tại!";
                TempData["MessageType"] = "warning";
                return View();
            }
            string newPassword = Guid.NewGuid().ToString().Substring(0, 8);
            user.MatKhau = newPassword;
            _nguoiDungRepository.Save();
            SendEmail(email, newPassword);
            TempData["Message"] = "Mật khẩu mới đã được gửi đến Email!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Login");
        }
        private void SendEmail(string email, string newPassword)
        {
            var sender = "webbansach2004@gmail.com";
            var appPassword = "iiee yeon rpet piht";
            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(sender, appPassword),
                EnableSsl = true,
            };
            smtp.Send(sender, email, "Mật khẩu mới từ Webbansach.com", $"Mật khẩu mới của bạn là: {newPassword}");
        }
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = _nguoiDungRepository.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        public IActionResult Edit()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NguoiDung user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            var existingUser = _nguoiDungRepository.GetById(user.MaNguoiDung);
            if (existingUser == null)
            {
                TempData["Message"] = "Người dùng không tồn tại!";
                TempData["MesseType"] = "error";
                return RedirectToAction("Index");
            }
            existingUser.TaiKhoan = user.TaiKhoan;
            existingUser.MatKhau = user.MatKhau;
            existingUser.HoTen = user.HoTen;
            existingUser.Email = user.Email;
            existingUser.SoDienThoai = user.SoDienThoai;
            existingUser.DiaChi = user.DiaChi;
            existingUser.VaiTro = user.VaiTro;

            _nguoiDungRepository.Save();

            TempData["Message"] = "Cập nhật thành công!";
            TempData["MesseType"] = "success";

            return RedirectToAction("Details", new { id = user.MaNguoiDung });
        }

    }
}
