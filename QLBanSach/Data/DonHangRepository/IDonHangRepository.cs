using QLBanSach.Models;

namespace QLBanSach.Data.DonHangRepository
{
    public interface IDonHangRepository
    {
        IEnumerable<DonHang> GetAll();
        IEnumerable<DonHang> GetIDNguoiDung(string manguoidung);
        void Add(DonHang donHang);
        void Update(DonHang donHang);
        void Delete(DonHang donHang);
        void Save();
        
    }
}
