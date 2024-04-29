using BookWebShop.Models.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWebShop.Models.ViewModels;

public class ShoppingCartViewModel
{
    [ValidateNever]
    public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
    public double TotalPrice { get; set; }
}
