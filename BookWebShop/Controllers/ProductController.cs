using Microsoft.AspNetCore.Mvc;
using BookWebShop.DataAccess.Data;
using BookWebShop.Models.Models;

namespace BookWebShop.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;
    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        List<Category> categorylist = _context.Categories.ToList();
        List<Product> productList = _context.Products.ToList();
        return View(productList);
    }

    public IActionResult Create()
    {
        return View();
    }


}
