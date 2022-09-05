using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AVILOT.AVQuestionsEngine.Database
{
    public class CollectionModel : QuestionModelVersions.CollectionModelV2 { }
    public class QuestionModel : QuestionModelVersions.QuestionModelV2 { }
    public class AnswerModel : QuestionModelVersions.AnswerModelV2 { }
}
namespace AVILOT.AVQuestionsEngine.Database.QuestionModelVersions
{
    [SQLite.Table("QuestionModel")]
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
    [SQLite.Table("QuestionModel")]
    public class QuestionModelV2
    {
        //database id
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        //main info
        public string Text { get; set; }
        public string ImageUrl { get; set; }
        [Indexed]
        public int? ParentCollectionId { get; set; }
        public int CorrectAnswerIndex { get; set; }
        //stats
        public DateTime? LastCorrectAnswer { get; set; }
        public DateTime? LastWrongAnswer { get; set; }
        public int AnsweredCorrectCount { get; set; }
        public int AnsweredWrongCount { get; set; }
    }
    [SQLite.Table("AnswerModel")]
    public class AnswerModelV1
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Text { get; set; }
        public bool Correct { get; set; }
        [Indexed]
        public int ParentQuestionId { get; set; }
    }
    [SQLite.Table("AnswerModel")]
    public class AnswerModelV2
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Text { get; set; }
        public string ImageUrl { get; set; }
        public string AnsweredExplanation { get; set; }
        public bool Correct { get; set; }
        [Indexed]
        public int ParentQuestionId { get; set; }
        public int AnswerIndex { get; set; }
    }
    [SQLite.Table("CollectionModel")]
    public class CollectionModelV1
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
    }
    [SQLite.Table("CollectionModel")]
    public class CollectionModelV2
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public string ImageUrl { get; set; }
        public string ImageBannerUrl { get; set; }
    }
}