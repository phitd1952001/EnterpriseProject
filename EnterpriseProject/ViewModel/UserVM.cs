using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EnterpriseProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnterpriseProject.ViewModels
{
    public class UserVM
    {
        
        [Required]
        public string Role { get; set; }
        [Required]
        public int DepartmentId { get; set; }
        public IEnumerable<SelectListItem> Rolelist { get; set; }
        public IEnumerable<SelectListItem> Departmentlist { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}