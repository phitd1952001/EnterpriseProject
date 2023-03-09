using System;
using System.Linq;
using System.Security.Claims;
using EnterpriseProject.Data;
using EnterpriseProject.Models;
using EnterpriseProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseProject.Areas.Authenticated.Controllers
{
    [Area("Authenticated")]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CommentsController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET
        public IActionResult Index(int ideaId)
        {
            var listComments = _db.Comments.Where(_=>_.IdeaId == ideaId).Include(u => u.ApplicationUser).ToList();

            var vm = new CommentVM()
            {
                Comments = listComments,
                IdeaId = ideaId
            };
            
            return View(vm);
        }
        
        [HttpPost]
        public IActionResult AddComment(CommentVM commentVm)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            var comment = new Comment()
            {
                IdeaId = commentVm.IdeaId,
                Text = commentVm.Text,
                DateTime = DateTime.Now,
                UserId = claims.Value
            };

            _db.Comments.Add(comment);
            _db.SaveChanges();
            
            return RedirectToAction(nameof(Index), new {ideaId = commentVm.IdeaId});
        }
    }
}