using BookWebShop.DataAccess.Data;
using BookWebShop.DataAccess.Repository.IRepository;
using BookWebShop.DataAccess.Repository;
using BookWebShop.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWebShop.DataAccess.Repository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private ApplicationDbContext _context;
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
    public void Update(Product product)
    {
        //_context.Update(product);

        var productInDb = _context.Products.FirstOrDefault(p => p.Id == product.Id);

        if (productInDb != null)
        {
            productInDb.ISBN = product.ISBN;
            productInDb.Title = product.Title;
            productInDb.Author = product.Author;
            productInDb.Description = product.Description;
            productInDb.CategoryId = product.CategoryId;
            productInDb.ListPrice = product.ListPrice;
            productInDb.Price = product.Price;
            productInDb.Price50 = product.Price50;
            productInDb.Price100 = product.Price100;

            if (productInDb.ImageUrl != null)
            {
                productInDb.ImageUrl = product.ImageUrl;
            }
        }
    }
}
