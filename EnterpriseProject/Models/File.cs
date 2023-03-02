using System.ComponentModel.DataAnnotations;

namespace EnterpriseProject.Models
{
    public class File
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
    }
}