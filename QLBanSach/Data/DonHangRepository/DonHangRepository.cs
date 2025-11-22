using Microsoft.CodeAnalysis.CSharp.Syntax;
using QLBanSach.Models;

namespace QLBanSach.Data.DonHangRepository
{
    public class DonHangRepository : IDonHangRepository
    {
        private readonly AppDbContext _context;

        public DonHangRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<DonHang> GetAll()=>_context.DonHang.ToList();

        public IEnumerable<DonHang> GetIDNguoiDung(string manguoidung)=>_context.DonHang
                           .Where(dh => dh.MaNguoiDung == manguoidung)
                           .ToList();
        public void Add(DonHang donHang)=>_context.DonHang.Add(donHang);

        public void Update(DonHang donHang)=>_context.DonHang.Update(donHang);

        public void Delete(DonHang donHang)=>_context.DonHang.Remove(donHang);

        public void Save()=>_context.SaveChanges();
        
    }
}
