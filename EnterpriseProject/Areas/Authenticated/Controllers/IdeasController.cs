using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterpriseProject.Data;
using EnterpriseProject.Models;
using EnterpriseProject.Utility;
using EnterpriseProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using File = EnterpriseProject.Models.File;

namespace EnterpriseProject.Areas.Authenticated.Controllers
{
    [Area(SD.Authenticated)]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Manager + "," + SD.Role_Staff)]
    public class IdeasController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ISendMailService _emailSender;

        public IdeasController(ApplicationDbContext db,UserManager<IdentityUser> userManager, ISendMailService emailSender)
        {
            _db = db;
            _userManager = userManager;
            _emailSender = emailSender;
        }
        // GET
        public IActionResult Index(string searchString, int topicId)
        {
            ViewData["Message"] = TempData["Message"];
            var listIdea = _db.Ideas.Where(_=>_.TopicId == topicId).Include(i => i.ApplicationUser)
                .Include(i => i.Category)
                .Include(i => i.Topic).ToList();
            
            var reacts = new List<ReactVM>();
            
            if (!String.IsNullOrEmpty(searchString))
            {
                listIdea = listIdea.Where(s => s.Text.Contains(searchString)).ToList();
            }
            
            foreach (var idea in listIdea)
            {
                var like = _db.Reacts.Where(_ => _.IdeaId == idea.Id).Sum(_ => _.Like);
                var disLike = _db.Reacts.Where(_ => _.IdeaId == idea.Id).Sum(_ => _.DisLike);
                var view = _db.Views.Count(_ => _.IdeaId == idea.Id);
                var react = new ReactVM()
                {
                    View = view,
                    Idea = idea,
                    Like = like,
                    DisLike = disLike
                };
                
                reacts.Add(react);
            }

            var response = new TopicsVM()
            {
                ReactVms = reacts,
                Topic = _db.Topics.Find(topicId)
            };
            
            return View(response);
        }

        [HttpGet]
        public IActionResult Upsert(int? id, int topicId )
        {
            
            IdeaVM ideaVm = new IdeaVM()
            {
                CategoryList = categoriesSelectListItems(),
                TopicId = topicId
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
            ideaVm.FileName = _db.Files.Find(idea.FileId).Name;
            return View(ideaVm);
        }
        
        
        [HttpPost]
        public async Task<IActionResult> UpSert(IdeaVM ideaVm)
        {
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                
                if (ideaVm.IsAgree)
                {
                    if (ideaVm.Id == 0)
                    {
                            // SendEmail to QA coordinator
                        var currentUser = _db.ApplicationUsers.FirstOrDefault(i => i.Id == claims.Value);
                        var userList = _db.ApplicationUsers.Where(_ => _.DepartmentId == currentUser.DepartmentId).ToList();
                        ApplicationUser qaCoordinator = new ApplicationUser();
                        foreach (var user in userList)
                        {
                            var roleTemp = await _userManager.GetRolesAsync(user);
                            if (roleTemp.First() == SD.Role_Coordinator)
                            {
                                qaCoordinator = user;
                                break;
                            }
                        }
                        
                        // add file
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
                        
                        // create idea
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

                        //check idea được add thành công hay không và send mail to coordinator
                        if (qaCoordinator != null && qaCoordinator.Email != null)
                        {
                                await _emailSender
                                .SendEmailAsync(qaCoordinator.Email, "New Idea Is Added", 
                                    $"<h1>New Idea Is Added</h1>");
                        }
                        TempData["Message"] = "Success: Add Successfully";
                        return RedirectToAction(nameof(Index), new {topicId = ideaVm.TopicId});
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
                    return RedirectToAction(nameof(Index), new {topicId = ideaVm.TopicId});
                }
                else
                {
                    ViewData["Message"] = "Fail: You must agree Terms and Conditions before submit";
                }
            }

            ideaVm.CategoryList = categoriesSelectListItems();
            return View(ideaVm);
        }

        public IActionResult Delete(int? id)
        {
            var ideaWantToDelete = _db.Ideas.Find(id);
            _db.Ideas.Remove(ideaWantToDelete);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index), new {topicId = ideaWantToDelete.TopicId});
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

        public async Task<IActionResult> Download(int id)
        {
            var fileModel = await _db.Files.FindAsync(id);
            if (fileModel == null)
                return NotFound();

            return File(fileModel.Data, fileModel.ContentType, fileModel.Name);
        }
    }
    
}