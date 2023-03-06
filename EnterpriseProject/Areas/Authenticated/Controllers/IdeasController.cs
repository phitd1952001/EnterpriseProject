﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterpriseProject.Data;
using EnterpriseProject.Models;
using EnterpriseProject.Utility;
using EnterpriseProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using File = EnterpriseProject.Models.File;

namespace EnterpriseProject.Areas.Authenticated.Controllers
{
    [Area(SD.Authenticated)]
    public class IdeasController : Controller
    {
        private readonly ApplicationDbContext _db;

        public IdeasController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET
        public IActionResult Index()
        {
            var listIdea = _db.Ideas.Include( i => i.ApplicationUser).ToList();
            return View(listIdea);
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            IdeaVM ideaVm = new IdeaVM()
            {
                CategoryList = categoriesSelectListItems(),
                TopicList = topicSelectListItems()
            };
            
            if (id == null)
            {
                return View(ideaVm);
            }

            var idea = _db.Ideas.Find(id);
            ideaVm.Id = idea.Id;
            ideaVm.Text = idea.Text;
            ideaVm.DateTime = idea.DateTime;
            ideaVm.CategoryId = idea.CategoryId;
            ideaVm.TopicId = idea.TopicId;

            return View(ideaVm);
        }
        
        [HttpPost]
        public async Task<IActionResult> UpSert(IdeaVM ideaVm)
        {
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                if (ideaVm.Id == 0)
                {
                    if (ideaVm.File == null || ideaVm.File.Length == 0)
                        return BadRequest("No file selected");

                    var fileModel = new File
                    {
                        Name = ideaVm.File.FileName,
                        ContentType = ideaVm.File.ContentType
                    };

                    using (var memoryStream = new MemoryStream())
                    {
                        await ideaVm.File.CopyToAsync(memoryStream);
                        fileModel.Data = memoryStream.ToArray();
                    }
                    _db.Files.Add(fileModel);
                    _db.SaveChanges();
                    
                    var ideaCreate = new Idea()
                    {
                        Text = ideaVm.Text,
                        DateTime = ideaVm.DateTime,
                        FileId = fileModel.Id,
                        CategoryId = ideaVm.CategoryId,
                        TopicId = ideaVm.TopicId,
                        UserId = claims.Value
                    };
                    _db.Ideas.Add(ideaCreate);
                    _db.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }

                var idea = _db.Ideas.Find(ideaVm.Id);
                idea.Text = ideaVm.Text;
                idea.DateTime = ideaVm.DateTime;
                idea.CategoryId = ideaVm.CategoryId;
                idea.TopicId = ideaVm.TopicId;
                idea.UserId = claims.Value;

                if (ideaVm.File != null && ideaVm.File.Length != 0)
                {
                    var fileModel = new File
                    {
                        Name = ideaVm.File.FileName,
                        ContentType = ideaVm.File.ContentType
                    };

                    using (var memoryStream = new MemoryStream())
                    {
                        await ideaVm.File.CopyToAsync(memoryStream);
                        fileModel.Data = memoryStream.ToArray();
                    }
                    // luu file new
                    _db.Files.Add(fileModel);
                    // xoa file cu
                    var oldFile = _db.Files.Find(idea.FileId);
                    _db.Remove(oldFile);
                    _db.SaveChanges();
                    idea.FileId = fileModel.Id;
                }
                
                _db.Update(idea);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ideaVm.CategoryList = categoriesSelectListItems();
            ideaVm.TopicList = topicSelectListItems();
            return View(ideaVm);
        }

        public IActionResult Delete(int? id)
        {
            var ideaWantToDelete = _db.Ideas.Find(id);
            _db.Ideas.Remove(ideaWantToDelete);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        
        [NonAction]
        private IEnumerable<SelectListItem> categoriesSelectListItems()
        {
            var categories = _db.Categories.ToList();
            var result = categories.Select(i => new SelectListItem()
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            return result;
        }

        [NonAction] 
        private IEnumerable<SelectListItem> topicSelectListItems()
        {
            var topics = _db.Topics.ToList();
            var result = topics.Select(i => new SelectListItem()
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            return result;
        }
        
        public async Task<IActionResult> Download(int id)
        {
            var fileModel = await _db.Files.FindAsync(id);
            if (fileModel == null)
                return NotFound();

            return File(fileModel.Data, fileModel.ContentType, fileModel.Name);
        }
    }
    
}