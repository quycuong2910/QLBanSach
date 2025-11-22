using Microsoft.EntityFrameworkCore;
using QLBanSach.Models;

namespace QLBanSach.Data.ChiTietGioHangRepository
{
    public class ChiTietGioHangRepository : IChiTietGioHangRepository
    {
        private readonly AppDbContext _context;

        public ChiTietGioHangRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(ChiTietGioHang chitietgiohang) => _context.ChiTietGioHang.Add(chitietgiohang);

        public ChiTietGioHang CheckSach(string name) => _context.ChiTietGioHang.FirstOrDefault(x=>x.MaSach==name);

        public IEnumerable<ChiTietGioHang> GetAll() => _context.ChiTietGioHang.ToList();

        public ChiTietGioHang GetById(string id) => _context.ChiTietGioHang.Find(id);

        public IEnumerable<ChiTietGioHang> GetByIDGioHang(string magiohang) => _context.ChiTietGioHang.Where(ct => ct.MaGioHang == magiohang).ToList();

        public void Save() => _context.SaveChanges();

        public void Update(ChiTietGioHang chiTietGioHang) => _context.ChiTietGioHang.Update(chiTietGioHang);
        public void CapNhatSoLuong(string maChiTiet, int soLuong)
        {
            var chiTiet = _context.ChiTietGioHang.Find(maChiTiet);
            if (chiTiet != null)
            {
                chiTiet.SoLuong = soLuong;
                _context.ChiTietGioHang.Update(chiTiet);
                _context.SaveChanges();
            }
        }

        public void Delete(ChiTietGioHang chiTietGioHang) => _context.ChiTietGioHang.Remove(chiTietGioHang);
        public decimal GetPrice(string madongiohang)
        {
            var chiTietGioHang = _context.ChiTietGioHang
            .Include(c => c.Sach)
            .Where(c => c.MaGioHang == madongiohang)
            .ToList();
            return chiTietGioHang.Sum(c => c.Sach.Gia * c.SoLuong);
        }
    }
}
