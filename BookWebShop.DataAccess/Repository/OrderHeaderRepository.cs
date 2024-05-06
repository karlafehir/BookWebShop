using BookWebShop.DataAccess.Data;
using BookWebShop.DataAccess.Repository.IRepository;
using BookWebShop.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWebShop.DataAccess.Repository;

public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
{
    private ApplicationDbContext _context;
    public OrderHeaderRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
    public void Update(OrderHeader orderHeader)
    {
        _context.Update(orderHeader);
    }
}
