using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BookWebShop.Models.Models;
using BookWebShop.Models.Models;
using BookWebShop.DataAccess.Repository.IRepository;

namespace BookWebShop.Areas.Customer.Controllers;
[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
        return View(productList);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Details(int productId)
    {
        Product product = _unitOfWork.Product.Get(p=> p.Id == productId, includeProperties: "Category");
        return View(product);
    }
}
