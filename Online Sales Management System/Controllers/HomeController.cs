using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Models;
using System.Diagnostics;

namespace OnlineSalesManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // 1. Lấy Categories Trending
        var trendingCats = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive && c.IsTrending)
            .OrderByDescending(c => c.Id)
            .Take(8)
            .ToListAsync();

        // 2. Lấy Products Trending
        var trendingProds = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category) // Load Category để hiển thị tên
            .Where(p => p.IsActive && p.IsTrending)
            .OrderByDescending(p => p.Id)
            .Take(8) // Lấy 8 sản phẩm
            .ToListAsync();

        // 3. Gói vào ViewModel
        var viewModel = new HomeViewModel
        {
            TrendingCategories = trendingCats,
            TrendingProducts = trendingProds
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}