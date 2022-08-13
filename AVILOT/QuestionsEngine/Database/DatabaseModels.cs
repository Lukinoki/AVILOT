using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AVILOT.AVQuestionsEngine.Database
{
    public class CollectionModel : QuestionModelVersions.CollectionModelV1 { }
    public class QuestionModel : QuestionModelVersions.QuestionModelV1 { }
    public class AnswerModel : QuestionModelVersions.AnswerModelV1 { }
}
namespace AVILOT.AVQuestionsEngine.Database.QuestionModelVersions
{
    public class QuestionModelV1
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Text { get; set; }
        [Indexed]
        public int? ParentCollectionId { get; set; }

        public int CorrectAnswerIndex { get; set; }
        public DateTime? LastCorrectAnswer { get; set; }
        public DateTime? LastWrongAnswer { get; set; }

        public int AnsweredCorrectCount { get; set; }
        public int AnsweredWrongCount { get; set; }
    }
    public class AnswerModelV1
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Text { get; set; }
        public bool Correct { get; set; }
        [Indexed]
        public int ParentQuestionId { get; set; }
    }

    public class CollectionModelV1
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}