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
        public Question question { get; set; }
        public bool correct { get; set; }
        public void select()
        {

        }
        public static Answer fromAnswerModel(DataModels.AnswerModel model, Question _question, bool _correct)
        {
            var answer = model as Answer;
            answer.correct = _correct;
            answer.question = _question;
            return answer;
        }
    }
    public class Question : DataModels.QuestionModel
    {
        QuestionsCollection? parentCollection { get; set; }
        Answer[] answers { get; set; }
        public void answer(int index)
        {

        }
        public static Question fromQuestionModel(DataModels.QuestionModel model, QuestionsCollection pcolection)
        {
            var question = model as Question;
            question.answers = new Answer[model.answerModels.Length];
            for (int i = 0; i < model.answerModels.Length; i++)
            {
                question.answers[i] = Answer.fromAnswerModel(model.answerModels[i], question, i == model.rightAnswerIndex);
            }
            return question;
        }
        
        

    }
    public class QuestionsCollection
    {
        public Question[] questions { get; set; }
        public int questionsCount { get; }
        public float answeredCorrectPercentage { get; }
        public int answeredCorrectCount { get; }
        public int answeredWrongCount { get; }
        public string name { get; set; }

        public Question getQuestion(int id)
        {

        }
        public Question getMostValuableQuestion()
        {

        }
        public void answerQuestion(Question question, int answerIndex)
        {

        }
        public static QuestionsCollection fromQuestionModelsList(string name, DataModels.QuestionModel[] qmodels)
        {
            QuestionsCollection collection = new QuestionsCollection();
            collection.name = name;
            var questions = new Question[qmodels.Length];
            for (int i = 0; i < qmodels.Length; i++)
            {
                questions[i] = Question.fromQuestionModel(qmodels[i], collection);
            }
            collection.questions = questions;
            return collection;
        }
    }

    
}
