using Microsoft.EntityFrameworkCore;
using QLBanSach.Models;
using System.Xml.Serialization;

namespace QLBanSach.Data.ChiTietGioHangRepository
{
    public interface IChiTietGioHangRepository
    {
        IEnumerable<ChiTietGioHang> GetAll();
        IEnumerable<ChiTietGioHang> GetByIDGioHang(string magiohang);
        void Add(ChiTietGioHang chitietgiohang);
        void Update(ChiTietGioHang chiTietGioHang);
        void Save();
        ChiTietGioHang CheckSach(string name);
        ChiTietGioHang GetById(string id);
        void CapNhatSoLuong(string maChiTiet, int soLuong);
        void Delete(ChiTietGioHang chiTietGioHang);
        decimal GetPrice(string madongiohang);
    }
}
