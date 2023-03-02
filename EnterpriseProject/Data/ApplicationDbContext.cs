using System;
using System.Collections.Generic;
using System.Text;
using EnterpriseProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseProject.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Idea> Ideas { get; set; }
        public DbSet<React> Reacts { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<View> Views { get; set; }
        public DbSet<File> Files { get; set; }
    }
}