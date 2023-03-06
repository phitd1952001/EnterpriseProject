using System.Linq;
using System.Security.Claims;
using EnterpriseProject.Data;
using EnterpriseProject.Models;
using EnterpriseProject.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace EnterpriseProject.Areas.Authenticated.Controllers
{
    [Area(SD.Authenticated)]
    public class ReactController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ReactController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET
        public IActionResult Index()
        {
            var listIdea = _db.Ideas.Include(i => i.ApplicationUser).ToList();
            return View(listIdea);
        }

        [HttpGet]
        public IActionResult Like(int ideaId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var react = _db.Reacts.FirstOrDefault(i => i.UserId == claims.Value && i.IdeaId == ideaId);
            if (react == null)
            {
                var newReact = new React()
                {
                    Like = 1,
                    UserId = claims.Value,
                    IdeaId = ideaId
                };
                _db.Reacts.Add(newReact);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult DisLike(int ideaId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var react = _db.Reacts.FirstOrDefault(i => i.UserId == claims.Value && i.IdeaId == ideaId);
            if (react == null)
            {
                var NewReact = new React()
                {
                    DisLike = 1,
                    UserId = claims.Value,
                    IdeaId = ideaId
                };
                _db.Reacts.Add(NewReact);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}