using System;
using System.Collections.Generic;
using System.Text;

namespace AVILOT.QuestionsEngine.DataModels
{
    internal class DataModels
    {
    }

    public class AnswerModel
    {
        string Text { get; set; }
    }

    public class QuestionModel
    {
        public int? id { get; set; }
        public string question { get; set; }
        public int rightAnswerIndex { get; set; }

        public AnswerModel[] answerModels { get; set; }
        public string correctAnswer { get { return answers[rightAnswerIndex]; } }
        public DateTime? lastRight { get; set; }
        public DateTime? lastWrong { get; set; }
        public string category { get; set; }

        public int wrongs = 0;
        public int rights = 0;
    }

}
