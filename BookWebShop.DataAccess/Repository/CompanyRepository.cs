using BookWebShop.DataAccess.Data;
using BookWebShop.DataAccess.Repository.IRepository;
using BookWebShop.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWebShop.DataAccess.Repository;

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    private ApplicationDbContext _context;
    public CompanyRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
    public void Update(Company company)
    {
        var companyInDb = _context.Companies.FirstOrDefault(p => p.Id == company.Id);

        if (companyInDb != null)
        {
            companyInDb.Name = company.Name;
            companyInDb.StreetAddress = company.StreetAddress;
            companyInDb.City = company.City;
            companyInDb.State = company.State;
            companyInDb.PostalCode = company.PostalCode;
            companyInDb.PhoneNumber = company.PhoneNumber;
        }
    }
}
