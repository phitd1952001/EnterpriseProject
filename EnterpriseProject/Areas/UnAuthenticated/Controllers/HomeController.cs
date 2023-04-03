using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EnterpriseProject.Models;
using EnterpriseProject.Utility;

namespace EnterpriseProject.Controllers
{
    [Area(SD.UnAuthenticated)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }
        
        
        public IActionResult Index()
        {
            IEnumerable<Topic> listTopic = _db.Topics.ToList();
            return View(listTopic);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

       
    }
}