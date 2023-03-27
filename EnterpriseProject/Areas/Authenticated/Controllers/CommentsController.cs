using System;
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var isUserViewed = _db.Views.Any(_ => _.IdeaId == ideaId && _.UserId == claims.Value);
            if (!isUserViewed)
            {
                _db.Views.Add(new View()
                {
                    VisitTime = DateTime.Now,
                    UserId = claims.Value,
                    IdeaId = ideaId
                });
                _db.SaveChanges();
            }
            
            var listComments = _db.Comments.Where(_=>_.IdeaId == ideaId).Include(u => u.ApplicationUser).ToList();
            var view = _db.Views.Count(_ => _.IdeaId == ideaId);
            
            var vm = new CommentVM()
            {
                Comments = listComments,
                View = view,
                IdeaId = ideaId,
                Idea = _db.Ideas.Where(_=>_.Id == ideaId).Include(_=>_.Topic).Include(_=>_.Category).Include(_=>_.ApplicationUser).FirstOrDefault()
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