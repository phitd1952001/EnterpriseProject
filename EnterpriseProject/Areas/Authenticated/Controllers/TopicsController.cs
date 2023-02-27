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
    public class TopicsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TopicsController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET
        public IActionResult Index(string searchString)
        {
            IEnumerable<Topic> listTopic = _db.Topics.ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                listTopic = listTopic.Where(c => c.Name.Contains(searchString));
            }
            return View(listTopic);
        }

        [HttpGet]
        public IActionResult UpSert(int? id)
        {
            if (id == null)
            {
                return View(new Topic());
            }

            var findTopic = _db.Topics.Find(id);
            return View(findTopic);
        }

        [HttpPost]
        public IActionResult UpSert(Topic topic)
        {
            if (ModelState.IsValid)
            {
                if (topic.Id == 0)
                {
                    _db.Topics.Add(topic);
                    _db.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }

                _db.Topics.Update(topic);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(topic);
        }

        public IActionResult Delete(int id)
        {
            var findId = _db.Topics.Find(id);
            _db.Remove(findId);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}