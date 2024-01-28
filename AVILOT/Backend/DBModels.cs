using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace AVILOT.Backend.Models
{
    public class Question
    {
        [PrimaryKey]
        public string question_id { get; set; }
        [NotNull]
        public string headline { get; set; }
        public string? explanation { get; set; }
        public int points { get; set; } = 1;
        public string? hint { get; set; }
        public int? code { get; set; }
        [NotNull]
        public string? media_type { get; set; }
        public string? media { get; set; }
        public string? media_flow { get; set; }
    }

    public class Answer
    {
        [PrimaryKey]
        public int answer_id { get; set; }
        [Indexed, NotNull]
        public string question { get; set; }
        public int order_index { get; set; }
        [NotNull]
        public string answer_text { get; set; }
        [NotNull]
        public bool correct { get; set; }
    }

    public class TestTemplate
    {
        [PrimaryKey]
        public string template_id { get; set; }
        [NotNull]
        public bool testable { get; set; } = true;
        [Indexed, NotNull]
        public string category { get; set; }
        [NotNull]
        public string headline { get; set; }
        [NotNull]
        public string about { get; set; }
        public int? question_count { get; set; }
        public int? minimum_points { get; set; }
        public TimeSpan? duration { get; set; }
    }

    public class Template_Question
    {
        [PrimaryKey]
        public int? id { get; set; } // must be unique
        [Indexed]
        public string question { get; set; }
        [Indexed]
        public string template { get; set; }
    }

    public class Category
    {
        [PrimaryKey]
        public string category_id { get; set; }
        public string? media { get; set; } // URL, icon, optional
        [NotNull]
        public string headline { get; set; }
    }

    public class Test
    {
        [PrimaryKey, AutoIncrement]
        public int? test_id { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        [Indexed]
        public string template { get; set; }
    }

    public class TestQuestion
    {
        [PrimaryKey, AutoIncrement]
        public int? id { get; set; }
        [NotNull]
        public string question { get; set; }
        [Indexed]
        public int test { get; set; }
    }

    public class PracticeAnswer
    {
        [PrimaryKey, AutoIncrement]
        public int? row_id { get; set; }
        [Indexed]
        public DateTime answered_time { get; set; }
        [Indexed]
        public int answer { get; set; }
        public int? test { get; set; }
    }

    public class QuestionBookmark
    {
        [PrimaryKey, AutoIncrement]
        public int? id { get; set; }
        [NotNull]
        public string question { get; set; }
    }
}
