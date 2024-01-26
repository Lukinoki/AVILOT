using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AVILOT.QEngine2;
using SQLite;
using AVILOT.AVQuestionsEngine;
using System.Linq;
using System.Threading.Tasks;
using AVILOT.AVQuestionsEngine.Database.QuestionModelVersions;

namespace AVILOT.QEngine2
{
    public class JsonLoad
    {
        public class UnreadableFileException : QuestionEngineException // TODO: change when exeptions are implemented
        { 
            public readonly string Filename;
            internal UnreadableFileException(string filename) : base($"Cannot read file: {filename}")
            {
                Filename = filename;
            }
            internal UnreadableFileException() : base($"Cannot read file")
            {
                Filename = "";
            }
        }
        

        public static async Task<bool> LoadJsonCollection(StreamReader reader, string filename, SQLiteAsyncConnection db, int CategoryId)
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
                var collection = new Collection()
                {
                    ParentCollectionId = CategoryId,
                    Name = config.name,
                    About = config.about,
                    ImageUrl = config.img,
                    ImageBannerUrl = config.bannerImg
                };
                await db.InsertAsync(collection);

                var questionsJArray = (JArray)jobject["questions"];
                var parsedQuestions = questionsJArray.ToObject<IEnumerable<JsonQuestionModelV2>>().ToList();
                foreach (var parsedQuestion in parsedQuestions)
                {
                    var question = new Question()
                    {
                        ParentCollectionId = collection.Id,
                        Text = parsedQuestion.txt,
                        CorrectAnswerIndex = parsedQuestion.cai,
                        ImageUrl = parsedQuestion.img,
                    };
                    await db.InsertAsync(question);

                    var correctAnswerIndex = parsedQuestion.cai;
                    var answerIndex = 0;
                    var questionAnswers = parsedQuestion.ans.Select(parsedAnswer =>
                    {
                        var answerModel = new Answer()
                        {
                            ParentQuestionId = question.Id,
                            Text = parsedAnswer.txt,
                            AnswerIndex = answerIndex,
                            Correct = answerIndex == correctAnswerIndex,
                            AnsweredExplanation = parsedAnswer.exp,
                            ImageUrl = parsedAnswer.img
                        };
                        answerIndex++;
                        return answerModel;
                    });
                    await db.InsertAllAsync(questionAnswers);
                }
                return true;
                
            }
            else
            {
                throw new UnreadableV2FileException(filename);
            }
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

    }

}
