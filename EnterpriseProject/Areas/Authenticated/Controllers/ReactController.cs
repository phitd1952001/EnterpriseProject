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
                return RedirectToAction("Index", "Ideas");
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
            return RedirectToAction("Index", "Ideas");
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
                return RedirectToAction("Index", "Ideas");
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

            return RedirectToAction("Index", "Ideas");
        }
    }
}