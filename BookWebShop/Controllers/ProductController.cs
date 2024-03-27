using Microsoft.AspNetCore.Mvc;
using BookWebShop.DataAccess.Data;
using BookWebShop.Models.Models;
using BookWebShop.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookWebShop.Models.ViewModels;

namespace BookWebShop.Controllers;

public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        List<Product> productList = _unitOfWork.Product.GetAll().ToList();
        return View(productList);
    }

    public IActionResult Upsert(int? productId)
    {
        IEnumerable<SelectListItem> categoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
        {
            Text = c.Name,
            Value = c.Id.ToString()
        });

        //1. nacin - najbrzi
        //ViewBag.CategoryList = categoryList; // asp -for= "Product.CategoryId" asp-items="ViewBag.categoryList"
        //2.nacin
        //ViewData["CategoryList"]= categoryList; // asp -for= "Product.CategoryId" asp-items="@(ViewData["CategoryList"]) as IEnumerable<SelectListItem>"
        //return View();

        //3.nacin - najcisci
        ProductViewModel productViewModel = new ProductViewModel()
        {
            CategoryList = categoryList,
            Product = new Product()
        };

        if (productId == null || productId == 0)
        {
            //Create
            return View(productViewModel);
        }
        else
        {
            //Update
            productViewModel.Product = _unitOfWork.Product.Get(p => p.Id == productId);
            return View(productViewModel);
        }

        // < select asp -for= "@Model.Product.CategoryId" asp - items = "@Model.CategoryList" class="form-select border-0 shadow">
    }

    [HttpPost]
    public IActionResult Upsert(ProductViewModel productViewModel, IFormFile file) // <input type="file" name="file
    {
        if (ModelState.IsValid)
        {
            //vodi nas do rute
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            //provjera jel file postoji
            if (file != null)
            {
                //generiramo filename
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                //tocni path di spremamo sliku
                string productPath = Path.Combine(wwwRootPath, @"images\product");

                //provjera jel slika vec postoji u bazi
                if(!string.IsNullOrEmpty(productViewModel.Product.ImageUrl))
                {
                    //nadi putanju postojece slike, porvjeri i obrisi ju
                    var oldImagePath = Path.Combine(wwwRootPath, productViewModel.Product.ImageUrl.Trim('\\')); //kod create  putanje file se doda dodatni \\ i moramo ga brisat

                    if(System.IO.File.Exists(oldImagePath)) 
                    { 
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                //using sam radi dispose, ne moramo naknadno disposeat
                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                productViewModel.Product.ImageUrl = @"\images\product\" + fileName;
            }

            if (productViewModel.Product.Id == 0)
            {
                _unitOfWork.Product.Add(productViewModel.Product);
            }
            else
            {
                _unitOfWork.Product.Update(productViewModel.Product);
            }

            _unitOfWork.Save();
            //TempData["success"] = "Product created Successfully";
            return RedirectToAction("Index", "Product");
        }
        //else nakon refresha da se ne isprazni dropdown
        else
        {
            productViewModel.CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });
        }

        return View(productViewModel);
    }

    public IActionResult Delete(int? productId)
    {
        if (productId == null || productId == 0)
        {
            return NotFound();
        }

        Product? product = _unitOfWork.Product.Get(c => c.Id == productId);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    //poziva se preko forme - dodali POST jer imamo 2 metode s istim imenom i parametrom
    [HttpPost, ActionName("Delete")]
    public IActionResult DeletePOST(int? productId)
    {
        Product? product = _unitOfWork.Product.Get(c => c.Id == productId);

        if (product == null)
        {
            return NotFound();
        }

        _unitOfWork.Product.Delete(product);
        _unitOfWork.Save();
        TempData["success"] = "Product deleted successfully";

        return RedirectToAction("Index", "Product");
    }
}
