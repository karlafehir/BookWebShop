﻿using BookWebShop.DataAccess.Data;
using BookWebShop.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWebShop.DataAccess.Repository;

public class UnitOfWork : IUnitOfWork
{
    ApplicationDbContext _context;
    public ICategoryRepository Category { get; private set; }


    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Category = new CategoryRepository(_context);
    }
    public void Save()
    {
        _context.SaveChanges();
    }
}
