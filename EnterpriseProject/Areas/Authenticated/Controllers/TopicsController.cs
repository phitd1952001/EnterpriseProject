using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using EnterpriseProject.Data;
using EnterpriseProject.Models;
using EnterpriseProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace EnterpriseProject.Areas.Authenticated.Controllers
{
    [Area(SD.Authenticated)]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Manager + "," + SD.Role_Staff)]
    
    public class TopicsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TopicsController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET
        public IActionResult Index(string searchString)
        {
            IEnumerable<Topic> listTopic = _db.Topics.ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                listTopic = listTopic.Where(c => c.Name.Contains(searchString));
            }
            return View(listTopic);
        }

        [HttpGet]
        public IActionResult UpSert(int? id)
        {
            if (id == null)
            {
                return View(new Topic());
            }

            var findTopic = _db.Topics.Find(id);
            return View(findTopic);
        }

        [HttpPost]
        public IActionResult UpSert(Topic topic)
        {
            if (ModelState.IsValid)
            {
                if (topic.Id == 0)
                {
                    _db.Topics.Add(topic);
                    _db.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }

                _db.Topics.Update(topic);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(topic);
        }

        public IActionResult Delete(int id)
        {
            var findId = _db.Topics.Find(id);
            _db.Remove(findId);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        
        public ActionResult DownloadZipFile(int topicId)
        {
            var ideas = _db.Ideas.Where(_ => _.TopicId == topicId);
            var fileIds = ideas.Select(_ => _.FileId).ToList();
            // retrieve zip file from database
            var zipFile = _db.Files.Where(f => fileIds.Contains(f.Id)).ToList();

            // Create a temporary directory to store the files
            var tempDirectory = Path.Combine(Path.GetTempPath(), _db.Topics.Find(topicId).Name);
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
            Directory.CreateDirectory(tempDirectory);

            // Save each file to the temporary directory
            foreach (var file in zipFile)
            {
                var filePath = Path.Combine(tempDirectory, file.Name);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    fileStream.Write(file.Data, 0, file.Data.Length);
                }
            }

            // Create a zip archive and add each file to it
            var zipFilePath = Path.Combine(tempDirectory, _db.Topics.Find(topicId).Name);//_db.Topics.Find(topicId).Name copy cái tên topic
            if (Directory.Exists(zipFilePath))
            {
                Directory.Delete(zipFilePath, true);
            }
            using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var file in zipFile)
                {
                    var fileEntry = zipArchive.CreateEntry(file.Name);
                    using (var entryStream = fileEntry.Open())
                    {
                        entryStream.Write(file.Data, 0, file.Data.Length);
                    }
                }
            }

            // Set the content type and headers for the response
            var contentType = "application/zip";
            var contentDisposition = "attachment; filename="+ _db.Topics.Find(topicId).Name+".zip";
            Response.Headers.Add("Content-Disposition", contentDisposition);

            // Download the zip file
            return PhysicalFile(zipFilePath, contentType);
        }
        
        public FileResult DownloadExcel(int topicId)
        {
            var listIdea = _db.Ideas.Where(_=>_.TopicId == topicId).Include(i => i.ApplicationUser)
                .Include(i => i.Category)
                .Include(i => i.Topic).ToList();
            
            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add a new worksheet to the Excel package
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Define the title row
                worksheet.Cells[1, 1].Value = "No.";
                worksheet.Cells[1, 2].Value = "Text";
                worksheet.Cells[1, 3].Value = "File Name";
                worksheet.Cells[1, 4].Value = "User Name";
                worksheet.Cells[1, 5].Value = "Topic Name";
                worksheet.Cells[1, 6].Value = "Category Name";
                worksheet.Cells[1, 7].Value = "View";
                worksheet.Cells[1, 8].Value = "Like";
                worksheet.Cells[1, 9].Value = "Dislike";

                // Add some sample data rows
                int no = 1;
                int index = 2;
                foreach (var idea in listIdea)
                {
                    var like = _db.Reacts.Where(_ => _.IdeaId == idea.Id).Sum(_ => _.Like);
                    var disLike = _db.Reacts.Where(_ => _.IdeaId == idea.Id).Sum(_ => _.DisLike);
                    var view = _db.Views.Count(_ => _.IdeaId == idea.Id);
                    worksheet.Cells[index, 1].Value = no;
                    worksheet.Cells[index, 2].Value = idea.Text;
                    worksheet.Cells[index, 3].Value = _db.Files.Find(idea.FileId).Name;
                    worksheet.Cells[index, 4].Value = idea.ApplicationUser.FullName;
                    worksheet.Cells[index, 5].Value = idea.Topic.Name;
                    worksheet.Cells[index, 6].Value = idea.Category.Name;
                    worksheet.Cells[index, 7].Value = view;
                    worksheet.Cells[index, 8].Value = like;
                    worksheet.Cells[index, 9].Value = disLike;
                    no++;
                    index++;
                }
                
                // Auto-fit the columns to the content
                worksheet.Cells.AutoFitColumns();

                // Convert the Excel package to a byte array
                var fileContents = package.GetAsByteArray();

                // Set the file name and content type for the Excel file
                var fileName = _db.Topics.Find(topicId).Name +".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                
                // Return the Excel file as a FileResult
                return File(fileContents, contentType, fileName);
            }
        }
    }
}