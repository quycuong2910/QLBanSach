using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace QLBanSach.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        public ThanhToanController(IMemoryCache cache, IConfiguration config)
        {
            _cache = cache;
            _config = config;
        }

        // Tạo mã thanh toán
        [HttpPost]
        public IActionResult TaoMaThanhToan([FromBody] ThanhToanRequest request)
        {
            try
            {
                // Tạo mã ngẫu nhiên
                string maThanhToan = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();

                // Lưu vào cache với thời gian hết hạn 15 phút
                var thongTin = new ThanhToanInfo
                {
                    MaDonHang = request.MaDonHang,
                    SoTien = request.SoTien,
                    NgayTao = DateTime.Now,
                    DaThanhToan = false
                };

                _cache.Set(maThanhToan, thongTin, TimeSpan.FromMinutes(15));

                Console.WriteLine($"Đã tạo mã thanh toán: {maThanhToan}");

                return Json(new
                {
                    success = true,
                    maThanhToan = maThanhToan,
                    hetHan = DateTime.Now.AddMinutes(15)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tạo mã thanh toán: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Sinh QR code
        [HttpGet]
        public IActionResult LayQRCode(string maThanhToan)
        {
            try
            {
                string publicUrl = _config["Payment:PublicUrl"];
                string callbackUrl = $"{publicUrl}/ThanhToan/XacNhanThanhToan?ma={maThanhToan}";

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(callbackUrl, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeBytes = qrCode.GetGraphic(20);

                return File(qrCodeBytes, "image/png");
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                Console.WriteLine($"Lỗi tạo QR: {ex.Message}");
                return StatusCode(500, "Không thể tạo QR Code");
            }
        }

        // Xác nhận thanh toán (được gọi từ điện thoại)
        [HttpGet]
        public IActionResult XacNhanThanhToan(string ma)
        {
            if (!_cache.TryGetValue(ma, out ThanhToanInfo thongTin))
            {
                return Content(@"
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1'>
                        <style>
                            body { font-family: Arial; text-align: center; padding: 50px; background: #f8f9fa; }
                            .container { background: white; padding: 30px; border-radius: 10px; max-width: 400px; margin: auto; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
                            .error { color: #dc3545; }
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h2 class='error'>❌ Mã thanh toán không hợp lệ</h2>
                            <p>Mã thanh toán đã được sử dụng, hết hạn hoặc không tồn tại.</p>
                        </div>
                    </body>
                    </html>
                ", "text/html");
            }

            if (thongTin.DaThanhToan)
            {
                return Content(@"
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1'>
                        <style>
                            body { font-family: Arial; text-align: center; padding: 50px; background: #f8f9fa; }
                            .container { background: white; padding: 30px; border-radius: 10px; max-width: 400px; margin: auto; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
                            .warning { color: #ffc107; }
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h2 class='warning'>⚠️ Đã thanh toán</h2>
                            <p>Mã thanh toán này đã được sử dụng trước đó.</p>
                        </div>
                    </body>
                    </html>
                ", "text/html");
            }

            // Cập nhật trạng thái
            thongTin.DaThanhToan = true;
            thongTin.NgayThanhToan = DateTime.Now;
            _cache.Set(ma, thongTin, TimeSpan.FromMinutes(15));

            return Content($@"
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <style>
                        body {{ font-family: Arial; text-align: center; padding: 50px; background: #f8f9fa; }}
                        .container {{ background: white; padding: 30px; border-radius: 10px; max-width: 400px; margin: auto; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .success {{ color: #28a745; }}
                        .info {{ background: #e7f3ff; padding: 15px; border-radius: 8px; margin: 20px 0; }}
                        .amount {{ font-size: 24px; color: #dc3545; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2 class='success'>✅ Thanh toán thành công!</h2>
                        <div class='info'>
                            <p><strong>Mã giao dịch:</strong><br>{ma}</p>
                            <p class='amount'>{thongTin.SoTien:N0} VNĐ</p>
                            <p style='color: #6c757d; font-size: 14px; margin-top: 15px;'>
                                {DateTime.Now:dd/MM/yyyy HH:mm:ss}
                            </p>
                        </div>
                        <p style='color: #6c757d; margin-top: 20px;'>Bạn có thể đóng trang này.</p>
                    </div>
                </body>
                </html>
            ", "text/html");
        }

        // Kiểm tra trạng thái thanh toán
        [HttpGet]
        public IActionResult KiemTraThanhToan(string maThanhToan)
        {
            if (!_cache.TryGetValue(maThanhToan, out ThanhToanInfo thongTin))
            {
                return Json(new
                {
                    success = false,
                    message = "Mã thanh toán không tồn tại hoặc đã hết hạn"
                });
            }

            return Json(new
            {
                success = true,
                daThanhToan = thongTin.DaThanhToan,
                ngayThanhToan = thongTin.NgayThanhToan,
                soTien = thongTin.SoTien
            });
        }
    }

    // Class đơn giản để lưu thông tin
    public class ThanhToanInfo
    {
        public string MaDonHang { get; set; }
        public decimal SoTien { get; set; }
        public DateTime NgayTao { get; set; }
        public bool DaThanhToan { get; set; }
        public DateTime? NgayThanhToan { get; set; }
    }

    public class ThanhToanRequest
    {
        public string MaDonHang { get; set; }
        public decimal SoTien { get; set; }
    }
}