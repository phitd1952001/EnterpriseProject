﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Manager + "," + SD.Role_Staff + "," + SD.Role_Coordinator)]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ISendMailService _emailSender;

        public CommentsController(ApplicationDbContext db, ISendMailService emailSender)
        {
            _db = db;
            _emailSender = emailSender;
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

            var idea = _db.Ideas.Where(_ => _.Id == ideaId).Include(_ => _.Topic).Include(_ => _.Category)
                .Include(_ => _.ApplicationUser).FirstOrDefault();

            var vm = new CommentVM()
            {
                FileName = _db.Files.Find(idea.FileId).Name,
                Comments = listComments,
                View = view,
                IdeaId = ideaId,
                Idea = idea
            };

            return View(vm);
        }
        
        [HttpGet]
        public IActionResult UpDate(int? id)
        {
            var findComment = _db.Comments.Find(id);
            return View(findComment);
        }
        
        [HttpPost]
        public IActionResult UpDate(Comment comment)
        {
            var findComment = _db.Comments.Find(comment.Id);
            findComment.Text = comment.Text;
            findComment.DateTime = DateTime.Now;
            _db.Comments.Update(findComment);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index), new {ideaId = findComment.IdeaId});
        }
        
        public IActionResult Delete(int id)
        {
            var deleteComment = _db.Comments.Find(id);
            _db.Remove(deleteComment);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index), new { ideaId = deleteComment.IdeaId});
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(CommentVM commentVm)
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

            var idea = _db.Ideas.FirstOrDefault(_ => _.Id == commentVm.IdeaId);
            var userOwnerIdea = _db.ApplicationUsers.Find(idea.UserId);
            if (userOwnerIdea != null && userOwnerIdea.Email != null)
            {
                await _emailSender
                    .SendEmailAsync(userOwnerIdea.Email, "New Comment for your idea",
                        $"<div style='color: black;'><h1 style='color: black;'>Have a new comment on your idea</h1><p style='color: black;'><strong>From:</strong> {_db.ApplicationUsers.Find(claims.Value).FullName} ({_db.ApplicationUsers.Find(claims.Value).Email})</p><p style='color: black;'><strong>Content new comments are:</strong> {comment.Text}</p><p style='color: black;'><strong>Further Comments in the Idea:</strong> {_db.Ideas.Find(idea.Id).Text}</p><p style='color: black;'><strong>Time for posting Comment:</strong> {_db.Ideas.Find(idea.Id).DateTime}</p><p style='color: black;'><strong>Best Regards!</strong></p></div></div>");
            }
            return RedirectToAction(nameof(Index), new {ideaId = commentVm.IdeaId});
        }
    }
}