using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseProject.Data;
using EnterpriseProject.Utility;
using EnterpriseProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseProject.Areas.Authenticated.Controllers
{
    [Area(SD.Authenticated)]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Manager)]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public StatisticsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        // GET
        public async Task<IActionResult> Index()
        {
            // idea by department
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
            
            // percentage idea by department
            
            // number of staff by department
            var allUsers = _db.ApplicationUsers.ToList();
            foreach (var user in allUsers)
            {
                var roleTemp = await _userManager.GetRolesAsync(user);
                user.Role = roleTemp.FirstOrDefault();
            }

            var staffs = allUsers.Where(_ => _.Role == SD.Role_Staff).ToList();
            var departmentAll = _db.Departments.ToList();

            var statisticUsers = new List<Statistic>();
            foreach (var department in departmentAll)
            {
                var statistic = new Statistic()
                {
                    DepartmentName = department.Name,
                    NumberOfIdeas = staffs.Count(_ => _.DepartmentId == department.Id)
                };
                
                statisticUsers.Add(statistic);
            }
            
            ViewBag.DataCtv = statisticUsers.Select(_=>_.NumberOfIdeas).ToArray();
            ViewBag.LabelCtv = statisticUsers.Select(_=>_.DepartmentName).ToArray();
          
            return View();
        }
    }
}