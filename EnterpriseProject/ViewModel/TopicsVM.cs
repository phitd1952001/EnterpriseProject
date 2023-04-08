using System;
using System.Collections.Generic;
using EnterpriseProject.Models;

namespace EnterpriseProject.ViewModels
{
    public class TopicsVM
    {
        public IEnumerable<ReactVM> ReactVms { get; set; }
        public Topic Topic { get; set; }
        public int FileId { get; set; }
    }
}