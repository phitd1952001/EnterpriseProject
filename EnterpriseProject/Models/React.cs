using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseProject.Models
{
    public class React
    {
        [Key] 
        public int Id { get; set; }
        public int Like { get; set; }
        public int DisLike { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int IdeaId { get; set; }
    }
}