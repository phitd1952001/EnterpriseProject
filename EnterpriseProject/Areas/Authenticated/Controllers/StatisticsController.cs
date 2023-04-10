using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using EnterpriseProject.Data;
using EnterpriseProject.Models;
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
            var ideas = _db.Ideas.ToList();
            var userIds = ideas.Select(_ => _.UserId);
            var users = _db.ApplicationUsers.Where(_ => userIds.Contains(_.Id)).ToList();
            var departmentIds = users.Select(_ => _.DepartmentId).ToList();
            var departments = _db.Departments.Where(_ => departmentIds.Contains(_.Id)).ToList();

            var statistics = new List<Statistic>();
            foreach (var department in departments)
            {
                var usersInDepartments = users.Where(_ => _.DepartmentId == department.Id).ToList();
                var userIdsInDepartments = usersInDepartments.Select(_ => _.Id).ToList();
                var numberOfIdeas = ideas.Count(_ => userIdsInDepartments.Contains(_.UserId));
                var statistic = new Statistic()
                {
                    DepartmentName = department.Name,
                    NumberOfIdeas = numberOfIdeas
                };
                
                statistics.Add(statistic);
            }

            var departmentNoIdeas = _db.Departments.Where(_ => !departmentIds.Contains(_.Id));
            if (departmentNoIdeas.Any())
            {
                foreach (var department in departmentNoIdeas)
                {
                    var statistic = new Statistic()
                    {
                        DepartmentName = department.Name,
                        NumberOfIdeas = 0
                    };
                
                    statistics.Add(statistic);
                }
            }
            
            ViewBag.Data = statistics.Select(_=>_.NumberOfIdeas).ToArray();
            ViewBag.Labels = statistics.Select(_=>_.DepartmentName).ToArray();
            return View();
        }
    }
}