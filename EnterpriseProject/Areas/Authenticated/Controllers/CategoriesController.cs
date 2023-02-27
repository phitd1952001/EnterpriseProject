using System;
using System.Collections.Generic;
using System.Linq;
using EnterpriseProject.Data;
using EnterpriseProject.Models;
using EnterpriseProject.Utility;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseProject.Areas.Authenticated.Controllers
{
    [Area(SD.Authenticated)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET
        public IActionResult Index(string searchString)
        {
            IEnumerable<Category> listCategories = _db.Categories.ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                listCategories = listCategories.Where(c => c.Name.Contains(searchString));
            }
            return View(listCategories);
        }
        
        [HttpGet]
        public IActionResult UpSert(int? id)
        {
            if (id == null)
            {
                return View(new Category());
            }

            var findCategory = _db.Categories.Find(id);
            return View(findCategory);
        }

        [HttpPost]
        public IActionResult UpSert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    _db.Categories.Add(category);
                    _db.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }

                _db.Categories.Update(category);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public IActionResult Delete( int id)
        {
            var deleteCategories = _db.Categories.Find(id);
            _db.Remove(deleteCategories);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}