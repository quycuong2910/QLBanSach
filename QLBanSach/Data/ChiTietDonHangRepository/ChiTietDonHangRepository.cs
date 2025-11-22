using QLBanSach.Models;

namespace QLBanSach.Data.ChiTietDonHangRepository
{
    public class ChiTietDonHangRepository :IChiTietDonHangRepository
    {
        private readonly AppDbContext _context;

        public ChiTietDonHangRepository(AppDbContext context)
        {
            _context = context;
        }
        public IEnumerable<ChiTietDonHang> GetAll()=> _context.ChiTietDonHang.ToList();
        public ChiTietDonHang GetById(string maChiTiet)=>_context.ChiTietDonHang.Find(maChiTiet);

        public IEnumerable<ChiTietDonHang> GetByMaDonHang(string maDonHang)=> _context.ChiTietDonHang
                           .Where(ct => ct.MaDonHang == maDonHang)
                           .ToList();

        public void Add(ChiTietDonHang chiTiet)=> _context.ChiTietDonHang.Add(chiTiet);
        public void Update(ChiTietDonHang chiTiet)=>_context.ChiTietDonHang.Update(chiTiet);
        public void Delete(ChiTietDonHang chiTiet)=>_context.ChiTietDonHang.Remove(chiTiet);
        public void Save()=>_context.SaveChanges();
    }
}
