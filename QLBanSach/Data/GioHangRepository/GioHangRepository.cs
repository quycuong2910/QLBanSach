using Microsoft.EntityFrameworkCore;
using QLBanSach.Models;

namespace QLBanSach.Data.GioHangRepository
{
    public class GioHangRepository : IGioHangRepository
    {
        private readonly AppDbContext _context;

        public GioHangRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(GioHang giohang) => _context.GioHang.Add(giohang);

        public void Delete(GioHang giohang) => _context.GioHang.Remove(giohang);

        public GioHang GetById(string id) => _context.GioHang.Find(id);

        public GioHang GetByIDND(string manguoidung) => _context.GioHang.FirstOrDefault(nd => nd.MaNguoiDung == manguoidung);

        public IEnumerable<GioHang> GetData() => _context.GioHang.ToList();


        public void Save() => _context.SaveChanges();

        public void Update(GioHang giohang) => _context.GioHang.Update(giohang);
    }
}
