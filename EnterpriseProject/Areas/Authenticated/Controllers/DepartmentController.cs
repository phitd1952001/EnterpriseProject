using System;
using System.Linq;
using EnterpriseProject.Data;
using EnterpriseProject.Models;
using EnterpriseProject.Utility;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseProject.Areas.Authenticated.Controllers
{
    [Area(SD.Authenticated)]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DepartmentController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET
        public IActionResult Index(string searchString)
        {
            var listDeparment = _db.Departments.ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                listDeparment = listDeparment.Where(s => s.Name.Contains(searchString)).ToList();
            }
            return View(listDeparment);
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            if (id == null)
            {
                return View(new Department());
            }

            var findDeparment = _db.Departments.Find(id);
            return View(findDeparment);
        }

        [HttpPost]
        public IActionResult Upsert(Department department)
        {
            if (ModelState.IsValid)
            {
                if (department.Id == 0)
                {
                    _db.Departments.Add(department);
                    _db.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }

                _db.Departments.Update(department);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(department);
        }

        public IActionResult Delete(int id)
        {
            var deleteId = _db.Departments.Find(id);
            _db.Departments.Remove(deleteId);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}