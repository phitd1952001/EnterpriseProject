﻿using EnterpriseProject.Models;

namespace EnterpriseProject.ViewModels
{
    public class ReactVM
    {
        public Idea Idea { get; set; }
        public string FileName { get; set; }
        public int Like { get; set; }
        public int DisLike { get; set; }
        public int View { get; set; }
    }
}