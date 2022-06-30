using System;
using System.Collections.Generic;
using System.Text;


namespace AVILOT.QuestionsEngine
{
    public static class QuestionsEngine
    {
        static QuestionsEngine()
        {

        }
        public static QuestionsCollection allQuestionsCollection { get; }
        public static QuestionsCollection answeredWrongCollection { get; }
        public static QuestionsCollection answeredRightCollection { get; }
        
        public static QuestionsCollection getCategory(string name)
        {

        }
        public static string[] getCategoryNames()
        {

        }


    }
    public class Answer : DataModels.AnswerModel
    {
        public Question? question { get; set; }
        public bool correct { get; set; }
        public void answer()
        {

        }
    }
    public class Question : DataModels.QuestionModel
    {
        QuestionsCollection? parentCollection { get; set; }
        Answer[] answers { get; set; }
        public void answer(int index)
        {

        }

    }
    public class QuestionsCollection
    {
        public Question[] questions = new Question[0];
        public int questionsCount { get; }
        public float answeredCorrectPercentage { get; }
        public int answeredCorrectCount { get; }
        public int answeredWrongCount { get; }

        public Question getQuestion(int id)
        {

        }
        public Question getMostValuableQuestion()
        {

        }
        public void answerQuestion(Question question, int answerIndex)
        {

        }
    }

    
}
