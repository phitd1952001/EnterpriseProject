using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseProject.Models
{
    public class Comment
    {
        [Key] public int Id { get; set; }
        public string Text { get; set; }
        public DateTime DateTime { get; set; }
        [Required] public int IdeaId { get; set; }
        [Required] public string UserId { get; set; }
        [ForeignKey("UserId")] public ApplicationUser ApplicationUser { get; set; }
        
    }
}