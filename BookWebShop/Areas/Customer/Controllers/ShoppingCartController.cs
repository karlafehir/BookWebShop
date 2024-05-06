using BookWebShop.DataAccess.Repository;
using BookWebShop.DataAccess.Repository.IRepository;
using BookWebShop.Models.Models;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using BookWebShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace BookWebShop.Areas.Customer.Controllers;
[Area("Customer")]
[Authorize]

public class ShoppingCartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    public ShoppingCartViewModel ShoppingCartViewModel { get; set; }
    
    public ShoppingCartController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public IActionResult Index()
    {     
        //get userId
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        // IEnumerable<ShoppingCart> shoppingCartList = _unitOfWork.ShoppingCart.GetAll(sp => sp.ApplicationUserId == userId, includeProperties:"Product").ToList();

        ShoppingCartViewModel = new ShoppingCartViewModel()
        {
            ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(sp => sp.ApplicationUserId == userId, includeProperties: "Product")
        };
        
        foreach (var shoppingCart in ShoppingCartViewModel.ShoppingCartList)
        {
            shoppingCart.Price = CalculatePriceByQuantity(shoppingCart);
            ShoppingCartViewModel.TotalPrice += (shoppingCart.Price * shoppingCart.Count);
        }
        
        return View(ShoppingCartViewModel);
    }
    
    private double CalculatePriceByQuantity(ShoppingCart shoppingCart)
    {
        int quantity = shoppingCart.Count;
        double price;

        if (quantity <= 50)
        {
            price = shoppingCart.Product.Price;
        }
        else if (quantity <= 100)
        {
            price = shoppingCart.Product.Price50;
        }
        else
        {
            price = shoppingCart.Product.Price100;
        }

        return price;
    }
    
    public IActionResult Plus(int cartId)
    {
        var cartFromDb = _unitOfWork.ShoppingCart.Get(sc => sc.Id == cartId);
        cartFromDb.Count += 1;

        _unitOfWork.ShoppingCart.Update(cartFromDb);
        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }
    
    public IActionResult Minus(int cartId)
    {
        var cartFromDb = _unitOfWork.ShoppingCart.Get(sc => sc.Id == cartId);
        
        //remove item if count is lower than 1
        if (cartFromDb.Count <= 1)
        {
            _unitOfWork.ShoppingCart.Delete(cartFromDb);
        }
        else
        {
            cartFromDb.Count -= 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
        }

        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }
    
    public IActionResult Remove(int cartId)
    {
        var cartFromDb = _unitOfWork.ShoppingCart.Get(sc => sc.Id == cartId);

        _unitOfWork.ShoppingCart.Delete(cartFromDb);
        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }
}
