﻿using BookWebShop.DataAccess.Repository.IRepository;
using BookWebShop.Models.Models;
using BookWebShop.Models.ViewModels;
using BookWebShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWebShop.Areas.Admin.Controllers;
[Area("Admin")]
//pristup page-u samo ako je logiran admin
[Authorize(Roles = Role.Role_Admin)]
public class CompanyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    public CompanyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public IActionResult Index()
    {
        List<Company> companyList = _unitOfWork.Company.GetAll().ToList();
        return View(companyList);
    }

    public IActionResult Upsert(int? id)
    {
        Company company = new Company();
        if (id == null || id == 0)
        {
            //Create
            return View(company);
        }
        else
        {
            //Update
            company = _unitOfWork.Company.Get(p => p.Id == id);
            return View(company);
        }
    }

    [HttpPost]
    public IActionResult Upsert(Company company)
    {
        if (ModelState.IsValid)
        {
            if (company.Id == 0)
            {
                _unitOfWork.Company.Add(company);
            }
            else
            {
                _unitOfWork.Company.Update(company);
            }

            _unitOfWork.Save();
            return RedirectToAction("Index", "Company");
        }

        return View();
    }

    #region API Calls

    [HttpGet]
    public IActionResult GetAll()
    {
        List<Company> companyList = _unitOfWork.Company.GetAll().ToList();
        return Json(new { data = companyList });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var company = _unitOfWork.Company.Get(p => p.Id == id);

        if (company == null)
        {
            return Json(new { success = false, message = "Error while deleting." });
        }

        _unitOfWork.Company.Delete(company);
        _unitOfWork.Save();
        return Json(new { success = true, message = "Company deleted successfully!" });
    }

    #endregion
}
