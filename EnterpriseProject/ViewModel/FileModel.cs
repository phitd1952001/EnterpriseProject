using EnterpriseProject.Models;

namespace EnterpriseProject.ViewModels
{
    public class FileModel
    {
        public string FileName { get; set; }
        public File FilePath { get; set; }
        public bool IsSelected { get; set; }
        public Topic Topic { get; set; }
        public Idea Idea { get; set; }
    }
}