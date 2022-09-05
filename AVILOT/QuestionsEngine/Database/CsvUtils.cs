using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AVILOT.AVQuestionsEngine.Database;
using CsvHelper;
using System.Linq;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using AVILOT.AVQuestionsEngine.Database.QuestionModelVersions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace AVILOT.AVQuestionsEngine.Database
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

        public static (QuestionModel[], AnswerModel[][], CollectionModel) LoadQuestionsFromCSV(string path)
        {
            var filename = Path.GetFileName(path);
            using var reader = new StreamReader(path);
            return LoadQuestionsFromCSV(reader, filename);
        }
        public static (QuestionModel[], AnswerModel[][], CollectionModel) LoadQuestionsFromCSV(Stream stream, string filename)
        {
            using var reader = new StreamReader(stream);
            return LoadQuestionsFromCSV(reader, filename);
        }
        public static (QuestionModel[], AnswerModel[][], CollectionModel) LoadQuestionsFromCSV(StreamReader reader, string filename, string name = null)
        {
            QuestionModel[] questions;
            AnswerModel[][] answers;
            CollectionModel collection;
            var extension = Path.GetExtension(filename);
            if (extension == ".csv")
            {
                (questions, answers) = LoadAndConvertV1CsvFileModels(reader, filename);
                string colname;
                if (name != null)
                {
                    colname = name;
                }
                else
                {
                    colname = filename;
                }
                collection = new CollectionModel()
                {
                    Name = colname,
                    About = "",
                };
            }
            else if (extension == ".json")
            {
                (questions, answers, collection) = LoadAndConvertV2JsonFileModels(reader, filename);
            }
            else
            {
                throw new UnreadableFileException(filename);
            }
            return (questions, answers, collection);
        }
        private static (QuestionModel[], AnswerModel[][]) LoadAndConvertV1CsvFileModels(StreamReader reader, string filename)
        {
            try
            {
                var (questionModelsV1, AnswerModelsV1) = LoadV1CsvFileModels(reader, filename);
                var questions = DbUtils.ConvertNewestVersionToGlobalQuestionModel(DbUtils.ConvertV1QuestionModelsToV2(questionModelsV1));
                var answers = AnswerModelsV1.Select(x => DbUtils.ConvertNewestVersionToGlobalAnswerModel(DbUtils.ConvertV1AnswerModelsToV2(x))).ToArray();
                return (questions, answers);
            }
            catch (CsvHelperException)
            {
                throw new UnreadableFileException(filename);
            }

        }
        private static (QuestionModelV1[], AnswerModelV1[][]) LoadV1CsvFileModels(StreamReader reader, string filename)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Encoding = Encoding.UTF8,
                Delimiter = ","
            };
            using var csv = new CsvReader(reader, config);

            //read first line of header
            csv.Read();
            if (csv.GetField(0) != "avilottransfer")
            {
                throw new UnreadableFileException();
            }
            var versionString = csv.GetField(1);

            //parse using correct version
            if (versionString != "1")
            {
                throw new UnreadableFileException(versionString);
            }
            csv.Read();
            var records = csv.GetRecords<AvilotFormatCsvRecordVersions.AvilotFormatV1CsvRecord>().ToArray();
            var questions = new QuestionModelV1[records.Count()];
            var answers = new AnswerModelV1[records.Count()][];
            int i = 0;
            try
            {
                foreach (var record in records)
                {
                    var correctAnswerIndex = record.correct;
                    answers[i] = new AnswerModelV1[]
                    {
                        new AnswerModelV1()
                        {
                            Text = record.answer_0,
                            Correct = (record.correct == 0)
                        },
                        new AnswerModelV1()
                        {
                            Text = record.answer_1,
                            Correct = (record.correct == 1)
                        },
                        new AnswerModelV1()
                        {
                            Text = record.answer_2,
                            Correct = (record.correct == 2)
                        }
                    };

                    questions[i] = new QuestionModelV1()
                    {
                        Text = record.question,
                        CorrectAnswerIndex = record.correct,
                        AnsweredCorrectCount = 0,
                        AnsweredWrongCount = 0
                    };
                    i++;
                }
            }
            catch
            {
                throw new UnreadableV1FileException();
            }

            return (questions, answers);
        }
        private static (QuestionModel[], AnswerModel[][], CollectionModel) LoadAndConvertV2JsonFileModels(StreamReader reader, string filename)
        {
            var (questionModelsV2, AnswerModelsV2, collectionV2) = LoadV2JsonFileModels(reader, filename);
            var questions = DbUtils.convertToNewest(questionModelsV2);
            var answers = AnswerModelsV2.Select(x => DbUtils.convertToNewest(x)).ToArray();
            var collection = DbUtils.convertToNewest(collectionV2);
            return (questions, answers, collection);
        }
#nullable enable
        [JsonObject]
        private class JsonConfigModel
        {
            public string version { get; set; }
            public string name { get; set; }
            public string? about { get; set; } 
            public string? img { get; set; }
            public string? bannerImg { get; set; }
        }
        [JsonObject]
        public class JsonAnswerModelV2
        {
            public string txt { get; set; }
            public string? img { get; set; }
            public string? exp { get; set; }
        }
        [JsonObject]
        private class JsonQuestionModelV2
        {
            public string txt { get; set; }
            public int cai { get; set; }
            public List<JsonAnswerModelV2> ans { get; set; }
            public string? img { get; set; }
            public DateTime? lca { get; set; }
            public DateTime? lwa { get; set; }
            public int? acc { get; set; }
            public int? awc { get; set; }
        }
#nullable disable
        public static (QuestionModelV2[], AnswerModelV2[][], CollectionModelV2) LoadV2JsonFileModels(StreamReader reader, string filename)
        {
            var jsonReader = new JsonTextReader(reader);
            var jobject = (JObject)JToken.ReadFrom(jsonReader);
            var config = jobject["config"].ToObject<JsonConfigModel>();
            if (config == null)
            {
                throw new UnreadableV2FileException(filename);
            }
            if (config.version == null || String.IsNullOrEmpty(config.name))
            {
                throw new UnreadableV2FileException(filename);
            }
            
            if (config.version == "2")
            {
                var questionsJArray = (JArray)jobject["questions"];
                var parsedQuestions = questionsJArray.ToObject<IEnumerable<JsonQuestionModelV2>>().ToArray();
                var questions = new QuestionModelV2[parsedQuestions.Length];
                var answers = new AnswerModelV2[parsedQuestions.Length][];
                for (int i = 0; i < parsedQuestions.Length; i++)
                {
                    var parsedQuestion = parsedQuestions[i];
                    var correctAnswerIndex = parsedQuestion.cai;
                    var answerIndex = 0;
                    var questionAnswers = parsedQuestion.ans.Select(parsedAnswer =>
                    {
                        var answerModel = new AnswerModelV2()
                        {
                            Text = parsedAnswer.txt,
                            AnswerIndex = answerIndex,
                            Correct = answerIndex == correctAnswerIndex,
                            AnsweredExplanation = parsedAnswer.exp,
                            ImageUrl = parsedAnswer.img
                        };
                        answerIndex++;
                        return answerModel;
                    }).ToArray();
                    var question = new QuestionModelV2()
                    {
                        Text = parsedQuestion.txt,
                        CorrectAnswerIndex = parsedQuestion.cai,
                        ImageUrl = parsedQuestion.img,
                        LastCorrectAnswer = parsedQuestion.lca,
                        LastWrongAnswer = parsedQuestion.lwa,
                        AnsweredCorrectCount = parsedQuestion.acc ?? 0,
                        AnsweredWrongCount = parsedQuestion.awc ?? 0,
                    };
                    questions[i] = question;
                    answers[i] = questionAnswers;
                }
                var collection = new CollectionModelV2()
                {
                    Name = config.name,
                    About = config.about,
                    ImageUrl = config.img,
                    ImageBannerUrl = config.bannerImg
                };
                return (questions, answers, collection);
            }
            else
            {
                throw new UnreadableV2FileException(filename);
            }
        }




    }
}
