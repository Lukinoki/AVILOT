using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AVILOT.QuestionsEngine.DataModels;
using CsvHelper;
using System.Linq;

namespace AVILOT.QuestionsEngine
{
    public static class CsvUtils
    {
        private class AvilotFormatCsvRecord
        {
            public string question { get; set; }
            public int correct { get; set; }
            public string answer_0 { get; set; }
            public string answer_1 { get; set; }
            public string answer_2 { get; set; }

        }
        public static QuestionModel[] loadQuestionModelsFromCSV(string path)
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<AvilotFormatCsvRecord>();
                QuestionModel[] questions = new QuestionModel[records.Count()];
                int i = 0;
                foreach (var record in records)
                {
                    var answermodels = new AnswerModel[] 
                    { 
                        new AnswerModel() {Text=record.answer_0}, 
                        new AnswerModel() {Text=record.answer_1}, 
                        new AnswerModel() {Text=record.answer_2}, 

                    };
                    questions[i] = new QuestionModel() 
                    {
                        question = record.question, 
                        rightAnswerIndex = record.correct, 
                        answerModels = answermodels 
                    };
                    i++;
                }
                return questions;
            }
        }
        public static QuestionsCollection loadQuestionsFromCsvAsCollection(string name, string path)
        {
            return QuestionsCollection.fromQuestionModelsList(name, loadQuestionModelsFromCSV(path));
        }
    }
}
