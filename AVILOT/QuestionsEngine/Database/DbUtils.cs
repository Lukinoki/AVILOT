using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using AVILOT.AVQuestionsEngine.Database.QuestionModelVersions;
using System.IO;
using SQLite;
using System.Diagnostics;

namespace AVILOT.AVQuestionsEngine.Database
{
    public static class DbUtils
    {
        private static QuestionsDatabase _database;
        public async static Task<QuestionsDatabase> GetMainDatabase()
        {
            Debug.WriteLine("Getting main database started");
            if (_database != null) //check if the database has already been loaded
            {
                Debug.WriteLine("Database already exists, returning");
                return _database;
            }
            // get path of possible databases
            var applicationFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var databasesesFolderPath = Path.Combine(applicationFolderPath, "database");
            Debug.WriteLine(databasesesFolderPath);
            //create databases folder if does not exist
            if (!Directory.Exists(databasesesFolderPath)) 
            {
                Debug.Print("creating databases folder because it does not exist");
                Directory.CreateDirectory(databasesesFolderPath);
            }
            

            // skip if newest version exists
            // else try to load from older databases 
            var MostRecentDbPath = Path.Combine(databasesesFolderPath, $"dbV2.db");
            if (File.Exists(MostRecentDbPath))
            {
                //do nothing because newest version already exists
                Debug.Print("newest database exists, skipping conversion");
            }
            else if (File.Exists(Path.Combine(applicationFolderPath, "dbV1.db"))) //upgrade from v1
            {
                Debug.Print("Converting V1 database to V2");
                await ConvertV1DbToV2(Path.Combine(applicationFolderPath, "dbV1.db"), Path.Combine(databasesesFolderPath, "dbV2.db"));
            }

            
            _database = new QuestionsDatabase(MostRecentDbPath);
            Debug.Print("returning database");
            return _database;




        }
        public async static Task<(CollectionModelV1[], QuestionModelV1[], AnswerModelV1[])> LoadEverythingFromV1Database(string path)
        {
            
            var db = new SQLiteAsyncConnection(path);// connect to the database
            // setup database
            await db.CreateTableAsync<AnswerModelV1>(SQLite.CreateFlags.ImplicitPK | SQLite.CreateFlags.AutoIncPK);
            await db.CreateTableAsync<QuestionModelV1>(SQLite.CreateFlags.ImplicitPK | SQLite.CreateFlags.AutoIncPK);
            await db.CreateTableAsync<CollectionModelV1>(SQLite.CreateFlags.ImplicitPK | SQLite.CreateFlags.AutoIncPK);
            // get tables
            var CollectionTable = db.Table<CollectionModelV1>();
            var QuestionTable = db.Table<QuestionModelV1>();
            var AnswerTable = db.Table<AnswerModelV1>();
            // get data
            var collections = await CollectionTable.ToArrayAsync();
            var questions = await QuestionTable.ToArrayAsync();
            var answers = await AnswerTable.ToArrayAsync();
            await db.CloseAsync();
            return (collections, questions, answers);
        }
        internal static (CollectionModelV2[], QuestionModelV2[], AnswerModelV2[])
        ConvertV1ModelsToV2(CollectionModelV1[] colModels, QuestionModelV1[] questModels, AnswerModelV1[] ansModels)
        {
            CollectionModelV2[] colModelsV2 = ConvertV1CollectionModelsToV2(colModels);
            QuestionModelV2[] questModelsV2 = ConvertV1QuestionModelsToV2(questModels);
            AnswerModelV2[] ansModelsV2 = ConvertV1AnswerModelsToV2(ansModels);
            return (colModelsV2, questModelsV2, ansModelsV2);
        }

        internal static AnswerModelV2[] ConvertV1AnswerModelsToV2(AnswerModelV1[] ansModels)
        {
            var ansModelsV2 = new AnswerModelV2[ansModels.Length];
            var ansIndexes = new Dictionary<int, int>(); // parentCollection : index
            for (int i = 0; i < ansModels.Length; i++)
            {
                var qm = ansModels[i];
                var answerIndex = 0;
                if (ansIndexes.ContainsKey(qm.Id))
                {
                    answerIndex = ansIndexes[qm.Id] + 1;
                    ansIndexes[qm.Id] = answerIndex;
                }
                else
                {
                    ansIndexes[qm.Id] = 0;
                }
                ansModelsV2[i] = new AnswerModelV2
                {
                    Id = qm.Id,
                    Text = qm.Text,
                    Correct = qm.Correct,
                    ParentQuestionId = qm.ParentQuestionId,
                    AnswerIndex = answerIndex
                };
            }

            return ansModelsV2;
        }
        public static QuestionModel[] convertToNewest(QuestionModelV2[] model)
        {
            return ConvertNewestVersionToGlobalQuestionModel(model);
        }
        public static QuestionModel[] convertToNewest(QuestionModelV1[] model)
        {
            return convertToNewest(ConvertV1QuestionModelsToV2(model));
        }
        public static AnswerModel[] convertToNewest(AnswerModelV2[] model)
        {
            return ConvertNewestVersionToGlobalAnswerModel(model);
        }
        public static AnswerModel[] convertToNewest(AnswerModelV1[] model)
        {
            return convertToNewest(ConvertV1AnswerModelsToV2(model));
        }
        public static CollectionModel convertToNewest(CollectionModelV2 model)
        {
            return new CollectionModel()
            {
                Id = model.Id,
                Name = model.Name,
                About = model.About,
                ImageBannerUrl = model.ImageBannerUrl,
                ImageUrl = model.ImageUrl
            };
        }
        public static CollectionModel convertToNewest(CollectionModelV1 model)
        {
            var v2model = new CollectionModelV2()
            {
                Name = model.Name,
                Id = model.Id,
            };
            return convertToNewest(v2model);
        }

        internal static AnswerModel[] ConvertNewestVersionToGlobalAnswerModel(AnswerModelV2[] ansModels)
        {
            var ansModelsV2 = new AnswerModel[ansModels.Length];
            for (int i = 0; i < ansModels.Length; i++)
            {
                var qm = ansModels[i];
                
                ansModelsV2[i] = new AnswerModel
                {
                    Id = qm.Id,
                    Text = qm.Text,
                    Correct = qm.Correct,
                    ParentQuestionId = qm.ParentQuestionId,
                    AnswerIndex = qm.AnswerIndex,
                    AnsweredExplanation = qm.AnsweredExplanation,
                    ImageUrl = qm.ImageUrl,
                };
            }

            return ansModelsV2;
        }
        internal static QuestionModel[] ConvertNewestVersionToGlobalQuestionModel(QuestionModelV2[] models)
        {
            var questModelsV2 = new QuestionModel[models.Length];
            for (int i = 0; i < models.Length; i++)
            {
                var qm = models[i];
                questModelsV2[i] = new QuestionModel
                {
                    Id = qm.Id,
                    Text = qm.Text,
                    ParentCollectionId = qm.ParentCollectionId,
                    CorrectAnswerIndex = qm.CorrectAnswerIndex,
                    LastCorrectAnswer = qm.LastCorrectAnswer,
                    LastWrongAnswer = qm.LastWrongAnswer,
                    AnsweredCorrectCount = qm.AnsweredCorrectCount,
                    AnsweredWrongCount = qm.AnsweredWrongCount,
                    ImageUrl = qm.ImageUrl,
                };
            }
            return questModelsV2;
        }
        internal static CollectionModel[] ConvertNewestVersionToGlobalCollectionModel(CollectionModelV2[] colModels)
        {
            var colModelsV2 = new CollectionModel[colModels.Length];
            for (int i = 0; i < colModels.Length; i++)
            {
                var colModel = colModels[i];
                colModelsV2[i] = new CollectionModel
                {
                    Id = colModel.Id,
                    Name = colModel.Name,
                    About = colModel.About,
                    ImageBannerUrl = colModel.ImageBannerUrl,
                    ImageUrl = colModel.ImageUrl
                };
            }

            return colModelsV2;
        }
        internal static QuestionModelV2[] ConvertV1QuestionModelsToV2(QuestionModelV1[] questModels)
        {
            var questModelsV2 = new QuestionModelV2[questModels.Length];
            for (int i = 0; i < questModels.Length; i++)
            {
                var qm = questModels[i];
                questModelsV2[i] = new QuestionModelV2
                {
                    Id = qm.Id,
                    Text = qm.Text,
                    ParentCollectionId = qm.ParentCollectionId,
                    CorrectAnswerIndex = qm.CorrectAnswerIndex,
                    LastCorrectAnswer = qm.LastCorrectAnswer,
                    LastWrongAnswer = qm.LastWrongAnswer,
                    AnsweredCorrectCount = qm.AnsweredCorrectCount,
                    AnsweredWrongCount = qm.AnsweredWrongCount,
                };
            }

            return questModelsV2;
        }

        internal static CollectionModelV2[] ConvertV1CollectionModelsToV2(CollectionModelV1[] colModels)
        {
            var colModelsV2 = new CollectionModelV2[colModels.Length];
            for (int i = 0; i < colModels.Length; i++)
            {
                var colModel = colModels[i];
                colModelsV2[i] = new CollectionModelV2
                {
                    Id = colModel.Id,
                    Name = colModel.Name,
                };
            }

            return colModelsV2;
        }

        private static async Task ConvertV1DbToV2(string v1Path, string v2path)
        {
            var (v1ColModels, v1QuestModels, v1AnsModels) = await LoadEverythingFromV1Database(v1Path);
            var (v2ColModels, v2QuestModels, v2AnsModels) = ConvertV1ModelsToV2(v1ColModels, v1QuestModels, v1AnsModels);
            var db = new SQLiteAsyncConnection(v2path);// connect to the database
            // setup database
            await db.CreateTableAsync<AnswerModelV2>(SQLite.CreateFlags.ImplicitPK);
            await db.CreateTableAsync<QuestionModelV2>(SQLite.CreateFlags.ImplicitPK);
            await db.CreateTableAsync<CollectionModelV2>(SQLite.CreateFlags.ImplicitPK);
            // import old data
            await db.InsertAllAsync(v2ColModels);
            await db.InsertAllAsync(v2QuestModels);
            await db.InsertAllAsync(v2AnsModels);
            // close db
            await db.CloseAsync();
        }
            
    }
}
