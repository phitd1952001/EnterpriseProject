using System;
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
    public class ReactController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ReactController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET
        public IActionResult Index(string searchString)
        {
            var listIdea = _db.Ideas.Include(i => i.ApplicationUser)
                .Include(i => i.Category)
                .Include(i => i.Topic).ToList();
            var reacts = new List<ReactVM>();
            
            foreach (var idea in listIdea)
            {
                var like = _db.Reacts.Where(_ => _.IdeaId == idea.Id).Sum(_ => _.Like);
                var disLike = _db.Reacts.Where(_ => _.IdeaId == idea.Id).Sum(_ => _.DisLike);
                var react = new ReactVM()
                {
                    Idea = idea,
                    Like = like,
                    DisLike = disLike
                };
                
                reacts.Add(react);
            }
            
            if (!String.IsNullOrEmpty(searchString))
            {
                listIdea = listIdea.Where(s => s.Text.Contains(searchString)).ToList();
            }
            
            
            return View(reacts);
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
            else
            {
                if (react.DisLike == 0 && react.Like == 0)
                {
                    react.Like = 1;
                }
                else if (react.DisLike == 0)
                {
                    react.Like = 0;
                }
                else
                {
                    react.DisLike = 0;
                    react.Like = 1;
                }
            }
            _db.Reacts.Update(react);
            _db.SaveChanges();
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
            else
            {
                if (react.DisLike == 0 && react.Like == 0)
                {
                    react.DisLike = 1;
                }
                else if (react.Like == 0)
                {
                    react.DisLike = 0;
                }
                else
                {
                    react.Like = 0;
                    react.DisLike = 1;
                }
            }
            _db.Reacts.Update(react);
            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}