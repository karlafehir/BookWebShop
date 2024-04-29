using BookWebShop.DataAccess.Repository;
using BookWebShop.DataAccess.Repository.IRepository;
using BookWebShop.Models.Models;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;

namespace BookWebShop.Areas.Customer.Controllers;
[Area("Customer")]

public class ShoppingCartController : Controller
{

    private readonly IUnitOfWork _unitOfWork;

    public ShoppingCartController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public IActionResult Index()
    {     
        //get userId
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        IEnumerable<ShoppingCart> shoppingCartList = _unitOfWork.ShoppingCart.GetAll(sp => sp.ApplicationUserId == userId, includeProperties:"Product").ToList();

        return View(shoppingCartList);
    }
}
