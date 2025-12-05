using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QLBanSach.Data;
using QLBanSach.Data.ChiTietDonHangRepository;
using QLBanSach.Data.ChiTietGioHangRepository;
using QLBanSach.Data.DonHangRepository;
using QLBanSach.Data.GioHangRepository;
using QLBanSach.Data.NguoiDungRepository;
using QLBanSach.Data.SachRepository;
using QLBanSach.Data.TheLoaiRepository;
using QLBanSach.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContext") ?? throw new InvalidOperationException("Connection string 'AppDbContext' not found.")));
builder.Services.AddScoped<INguoiDungRepository, NguoiDungRepository>();
builder.Services.AddScoped<ISachRepository,SachRepository>();
builder.Services.AddScoped<ITheLoaiRepository, TheLoaiRepository>();
builder.Services.AddScoped<IGioHangRepository, GioHangRepository>();
builder.Services.AddScoped<IDonHangRepository, DonHangRepository>();
builder.Services.AddScoped<IChiTietGioHangRepository, ChiTietGioHangRepository>();
builder.Services.AddScoped<IChiTietDonHangRepository, ChiTietDonHangRepository>();
builder.Services.AddSession();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
