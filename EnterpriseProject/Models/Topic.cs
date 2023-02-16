using System;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseProject.Models
{
    public class Topic
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime ClosureDate { get; set; }
        [Required]
        public DateTime FinalClosureDate { get; set; }
    }
}