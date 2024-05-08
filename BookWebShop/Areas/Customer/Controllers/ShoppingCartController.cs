using BookWebShop.DataAccess.Repository;
using BookWebShop.DataAccess.Repository.IRepository;
using BookWebShop.Models.Models;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using BookWebShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using BookWebShop.Utility;

namespace BookWebShop.Areas.Customer.Controllers;
[Area("Customer")]
[Authorize]

public class ShoppingCartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    [BindProperty]
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
            ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(sp => sp.ApplicationUserId == userId, includeProperties: "Product"),
            OrderHeader = new OrderHeader()
        };
        
        foreach (var shoppingCart in ShoppingCartViewModel.ShoppingCartList)
        {
            shoppingCart.Price = CalculatePriceByQuantity(shoppingCart);
            ShoppingCartViewModel.OrderHeader.OrderTotal += (shoppingCart.Price * shoppingCart.Count);
        }
        
        return View(ShoppingCartViewModel);
    }

    public IActionResult Summary()
    {
        //get userId
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartViewModel = new ShoppingCartViewModel()
        {
            ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(sp => sp.ApplicationUserId == userId, includeProperties: "Product"),
            OrderHeader = new OrderHeader()        
        };

        var applicationUser = new ApplicationUser();

        applicationUser = _unitOfWork.ApplicationUser.Get(au => au.Id == userId);

        ShoppingCartViewModel.OrderHeader.Name = applicationUser.Name;
        ShoppingCartViewModel.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
        ShoppingCartViewModel.OrderHeader.StreetAddress = applicationUser.StreetAddress;
        ShoppingCartViewModel.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
        ShoppingCartViewModel.OrderHeader.City = applicationUser.City;
        ShoppingCartViewModel.OrderHeader.State = applicationUser.State;
        ShoppingCartViewModel.OrderHeader.PostalCode = (int)applicationUser.PostalCode;

        foreach (var shoppingCart in ShoppingCartViewModel.ShoppingCartList)
        {
            shoppingCart.Price = CalculatePriceByQuantity(shoppingCart);
            ShoppingCartViewModel.OrderHeader.OrderTotal += (shoppingCart.Price * shoppingCart.Count);
        }

        return View(ShoppingCartViewModel);
    }

    [HttpPost]
    [ActionName("Summary")]
	public IActionResult SummaryPOST()
	{
        //get userId
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        ShoppingCartViewModel.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(sp => sp.ApplicationUserId == userId, includeProperties: "Product");

        ShoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
        ShoppingCartViewModel.OrderHeader.ApplicationUserId = userId;

        ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(au => au.Id == userId);

        foreach (var cart in ShoppingCartViewModel.ShoppingCartList)
        {
            cart.Price = CalculatePriceByQuantity(cart);
            ShoppingCartViewModel.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            //Customer
            ShoppingCartViewModel.OrderHeader.PaymentStatus = PaymentStatus.Pending;
            ShoppingCartViewModel.OrderHeader.OrderStatus = OrderStatus.Pending;
        }
        else
        {
            //Company
            ShoppingCartViewModel.OrderHeader.PaymentStatus = PaymentStatus.Delayed;
            ShoppingCartViewModel.OrderHeader.OrderStatus = OrderStatus.Approved;
        }

        _unitOfWork.OrderHeader.Add(ShoppingCartViewModel.OrderHeader);
        _unitOfWork.Save();

        foreach(var cart in ShoppingCartViewModel.ShoppingCartList)
        {
            OrderDetail orderDetail = new()
            {
                ProductId = cart.ProductId,
                OrderHeaderId = ShoppingCartViewModel.OrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count,
            };

            _unitOfWork.OrderDetail.Add(orderDetail);
            _unitOfWork.Save();
        }

        return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartViewModel.OrderHeader.Id });
	}

    public IActionResult OrderConfirmation(int id)
    {
        return View(id);
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
