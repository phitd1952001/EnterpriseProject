using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using EnterpriseProject.Data;
using EnterpriseProject.Models;
using EnterpriseProject.Utility;
using EnterpriseProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnterpriseProject.Areas.Authenticated.Controllers
{
    [Area(SD.Authenticated)]
    public class IdeasController : Controller
    {
        private readonly ApplicationDbContext _db;

        public IdeasController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET
        public IActionResult Index()
        {
            var listIdea = _db.Ideas.ToList();
            return View(listIdea);
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            IdeaVM ideaVm = new IdeaVM()
            {
                CategoryList = categoriesSelectListItems(),
                TopicList = topicSelectListItems()
            };
            
            if (id == null)
            {
                return View(ideaVm);
            }

            var idea = _db.Ideas.Find(id);
            ideaVm.Id = idea.Id;
            ideaVm.Text = idea.Text;
            ideaVm.DateTime = idea.DateTime;
            ideaVm.FilePath = idea.FilePath;
            ideaVm.CategoryId = idea.CategoryId;
            ideaVm.TopicId = idea.TopicId;

            return View(ideaVm);
        }
        
        [HttpPost]
        public IActionResult UpSert(IdeaVM ideaVm)
        {
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                if (ideaVm.Id == 0)
                {
                    var ideaCreate = new Idea()
                    {
                        Text = ideaVm.Text,
                        DateTime = ideaVm.DateTime,
                        FilePath = ideaVm.FilePath,
                        CategoryId = ideaVm.CategoryId,
                        TopicId = ideaVm.TopicId,
                        UserId = claims.Value
                    };
                    _db.Ideas.Add(ideaCreate);
                    _db.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }

                var idea = _db.Ideas.Find(ideaVm.Id);
                idea.Text = ideaVm.Text;
                idea.FilePath = ideaVm.FilePath;
                idea.DateTime = ideaVm.DateTime;
                idea.CategoryId = ideaVm.CategoryId;
                idea.TopicId = ideaVm.TopicId;
                idea.UserId = claims.Value;
                
                _db.Update(idea);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ideaVm.CategoryList = categoriesSelectListItems();
            ideaVm.TopicList = topicSelectListItems();
            return View(ideaVm);
        }

        public IActionResult Delete(int? id)
        {
            var ideaWantToDelete = _db.Ideas.Find(id);
            _db.Ideas.Remove(ideaWantToDelete);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        
        [NonAction]
        private IEnumerable<SelectListItem> categoriesSelectListItems()
        {
            var categories = _db.Categories.ToList();
            var result = categories.Select(i => new SelectListItem()
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            return result;
        }

        [NonAction] 
        private IEnumerable<SelectListItem> topicSelectListItems()
        {
            var topics = _db.Topics.ToList();
            var result = topics.Select(i => new SelectListItem()
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            return result;
        }
        
        
    }
}