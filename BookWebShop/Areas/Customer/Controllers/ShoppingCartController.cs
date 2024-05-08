using BookWebShop.DataAccess.Repository;
using BookWebShop.DataAccess.Repository.IRepository;
using BookWebShop.Models.Models;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using BookWebShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using BookWebShop.Utility;
using Stripe.BillingPortal;
using Stripe.Checkout;
using SessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using SessionService = Stripe.Checkout.SessionService;
using Session = Stripe.Checkout.Session;

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

        if (applicationUser.CompanyId.GetValueOrDefault() == 0)
        {
            var domain = "https://localhost:7076/";
            //Customer account - make payment
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"customer/shoppingcart/OrderConfirmation?id={ShoppingCartViewModel.OrderHeader.Id}",
                CancelUrl = domain + "customer/shoppingcart/index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in ShoppingCartViewModel.ShoppingCartList)
            {
                var sessionLineItem = new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = item.Product.Title,
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            //TODO - Add logic when shopping cart is empty
			var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartViewModel.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);
        }

        //
        return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartViewModel.OrderHeader.Id });
	}

    public IActionResult OrderConfirmation(int id)
    {
        //Check if payment is successful
        OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(oh => oh.Id == id, includeProperties: "ApplicationUser");

        //Customer payment (Only company payment can be delayed)
        if (orderHeader.PaymentStatus != PaymentStatus.Delayed)
        {
            var sessionService = new SessionService();
            Session session = sessionService.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
				_unitOfWork.OrderHeader.UpdateStatus(id, OrderStatus.Approved, PaymentStatus.Approved);
                _unitOfWork.Save();
			}
        }

        List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

        _unitOfWork.ShoppingCart.DeleteRange(shoppingCarts);
        _unitOfWork.Save();

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
