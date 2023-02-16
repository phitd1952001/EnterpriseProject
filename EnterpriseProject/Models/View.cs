using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseProject.Models
{
    public class View
    {
        [Key] public int Id { get; set; }
        public DateTime VisitTime { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public int IdeaId { get; set; }
    }
}