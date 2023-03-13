using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnterpriseProject.ViewModels
{
    public class IdeaVM
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string FilePath { get; set; }
        public DateTime DateTime { get; set; }
        public int CategoryId { get; set; }
        public int TopicId { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> TopicList { get; set; }
        public IFormFile File { get; set; }
    }
}