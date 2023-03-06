using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;

namespace EnterpriseProject.Models
{
    public class ApplicationUser: IdentityUser
    {
        [Required]
        public string FullName { get; set; } 
        [Required]
        public string PassportID  { get; set; }
        [Required]
        public DateTime Birthday { get; set; }
        [Required] 
        public string Address { get; set; }
        [NotMapped]
        public string Role { get; set; }
        
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
    }
}