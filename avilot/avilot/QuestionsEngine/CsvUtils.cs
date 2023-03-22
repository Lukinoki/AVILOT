using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using avilot.AVQuestionsEngine.Database;
using CsvHelper;
using System.Linq;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace avilot.AVQuestionsEngine
{
    public static class CsvUtils
    {
        
        private class AvilotFormatCsvRecordVersions
        {
            public class AvilotFormatV1CsvRecord
            {
                [Index(0)]
                public string question { get; set; }
                [Index(1)]
                public int correct { get; set; }
                [Index(2)]
                public string answer_0 { get; set; }
                [Index(3)]
                public string answer_1 { get; set; }
                [Index(4)]
                public string answer_2 { get; set; }
            }
        }
        public class WrongFileException : Exception { }
        public class UnknownTransferVersionException : Exception
        {
            readonly string Version;
            public UnknownTransferVersionException(string version) : base($"Unknown avilottransfer file version {version}")
            {
                Version = version;
            }
        }
        public class UnreadableFileException : Exception { }
        public static (Database.QuestionModel[], Database.AnswerModel[][]) LoadQuestionsFromCSV(string path)
        {
            using var reader = new StreamReader(path);
            return LoadQuestionsFromCSV(reader);
        }
        public static (Database.QuestionModel[], Database.AnswerModel[][]) LoadQuestionsFromCSV(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return LoadQuestionsFromCSV(reader);
        }
        public static (Database.QuestionModel[], Database.AnswerModel[][]) LoadQuestionsFromCSV(StreamReader reader)
        {
            Database.QuestionModel[] questions;
            Database.AnswerModel[][] answers;
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Encoding = Encoding.UTF8,
                Delimiter = ","
            };
            using (var csv = new CsvReader(reader, config))
            {
                //read first line of header
                csv.Read();
                Console.WriteLine(csv.GetField(0));
                if (csv.GetField(0) != "avilottransfer")
                {
                    throw new WrongFileException();
                }
                var versionString = csv.GetField(1);

                //parse using correct version
                if (versionString == "1")
                {
                    csv.Read();
                    var records = csv.GetRecords<AvilotFormatCsvRecordVersions.AvilotFormatV1CsvRecord>().ToArray();
                    questions = new Database.QuestionModel[records.Count()];
                    answers = new AnswerModel[records.Count()][];
                    int i = 0;

                    foreach (var record in records)
                    {
                        Console.WriteLine(csv.CurrentIndex);
                        var correctAnswerIndex = record.correct;
                        answers[i] = new AnswerModel[]
                        {
                        new Database.AnswerModel()
                        {
                            Text = record.answer_0,
                            Correct = (record.correct == 0)
                        },
                        new Database.AnswerModel()
                        {
                            Text = record.answer_1,
                            Correct = (record.correct == 1)
                        },
                        new Database.AnswerModel()
                        {
                            Text = record.answer_2,
                            Correct = (record.correct == 2)
                        }
                        };

                        questions[i] = new Database.QuestionModel()
                        {
                            Text = record.question,
                            CorrectAnswerIndex = record.correct,
                            AnsweredCorrectCount = 0,
                            AnsweredWrongCount = 0
                        };
                        i++;
                    }
                }
                else
                {
                    throw new UnknownTransferVersionException(versionString);
                }
            }
            return (questions, answers);

        }

    }
}
