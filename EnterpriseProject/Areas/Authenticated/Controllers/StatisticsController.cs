using System.Collections.Generic;
using System.Linq;
using EnterpriseProject.Data;
using EnterpriseProject.Utility;
using EnterpriseProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseProject.Areas.Authenticated.Controllers
{
    [Area(SD.Authenticated)]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Manager)]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public StatisticsController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET
        public IActionResult Index()
        {
            var topics = _db.Topics.ToList();
            var statistics = new List<Statistic>();
            foreach (var topic in topics)
            {
                var numberOfIdeas = _db.Ideas.Count(_ => _.TopicId == topic.Id);
                var statistic = new Statistic()
                {
                    TopicName = topic.Name,
                    NumberOfIdeas = numberOfIdeas
                };
                statistics.Add(statistic);
            }
            return View(statistics);
        }
    }
}