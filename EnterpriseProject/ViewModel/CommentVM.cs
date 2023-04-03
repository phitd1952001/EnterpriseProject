using System.Collections.Generic;
using EnterpriseProject.Models;

namespace EnterpriseProject.ViewModels
{
    public class CommentVM
    {
        public string Text { get; set; }
        public int IdeaId { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public Idea Idea { get; set; }
        public string FileName { get; set; }
        public int View { get; set; }
    }
}