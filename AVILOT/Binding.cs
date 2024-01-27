using System;
using System.Collections.Generic;
using System.Text;

namespace AVILOT
{
    public class Binding 
    {
        
        private static string[] ListOfCategories = { "New Test", "Wrong Answers", "Sequentially", "Sequentially (wrong)", "Questions", "Questions (wrong)" };
        public IEnumerable<string> Categories { get; set; } = ListOfCategories;



        public double Progress = 0.5;

        /*
        public class Category
        {
            public string Name { get; set; }
            public string Details { get; set; }
        }



        public Category[] Categories = new Category[]
        {
            new Category { Name = "Vithal Wadje", Details = "Mumbai" },
            new Category { Name = "Sudhir Wadje", Details = "Latur" },
            new Category { Name = "Anil", Details = "Delhi"}
        };
        */
    }
}
