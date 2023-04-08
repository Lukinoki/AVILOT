using System;
using System.Collections.Generic;
using System.IO;
sing System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace AVILOT.AVQuestionsEngine.Database
{
    public class QuestionsDatabase
    {
        private readonly SQLiteAsyncConnection db;
        public readonly AsyncTableQuery<CollectionModel> CollectionTable;
        public readonly AsyncTableQuery<QuestionModel> QuestionTable;
        public readonly AsyncTableQuery<AnswerModel> AnswerTable;
        public QuestionsDatabase(string dbPath)
        {
            db = new SQLiteAsyncConnection(dbPath);
            db.CreateTableAsync<AnswerModel>();
            db.CreateTableAsync<QuestionModel>();
            db.CreateTableAsync<CollectionModel>();
            CollectionTable = db.Table<CollectionModel>();
            QuestionTable = db.Table<QuestionModel>();
            AnswerTable = db.Table<AnswerModel>();
            
        }
        //utils
        public async Task ClearDatabase()
        {
            await CollectionTable.DeleteAsync(v => true);
            await QuestionTable.DeleteAsync(v => true);
            await AnswerTable.DeleteAsync(v => true);
        }
        public Task<CollectionModel> GetCollectionByName(string name)
        {
            return CollectionTable.FirstAsync(v => v.Name.Equals(name));
        }
        public Task<int> GetQuestionsCount()
        {
            return QuestionTable.CountAsync();
        }
        public Task<int> GetQuestionsCount(int collectionId)
        {
            return QuestionTable.CountAsync(v => v.Id.Equals(collectionId));
        }
        private string GenerateQuerryParametersFromList(int[] list)
        {
            return "(" + String.Join(",", list) + ")";
        }
        //get all
        public Task<CollectionModel[]> GetCollections()
        {
            return CollectionTable.ToArrayAsync();
        }
        public Task<QuestionModel[]> GetQuestions()
        {
            return QuestionTable.ToArrayAsync();
        }
        public Task<AnswerModel[]> GetAnswers()
        {
            return AnswerTable.ToArrayAsync();
        }

        public AsyncTableQuery<CollectionModel> _GetCollections() => CollectionTable;
        public AsyncTableQuery<CollectionModel> _GetCollections(int collectionId) => CollectionTable.Where(v => v.Id == collectionId);
        public AsyncTableQuery<CollectionModel> _GetCollections(string collectionName) => CollectionTable.Where(v => v.Name == collectionName);
        //building collections
        public Task<CollectionModel[]> GetSubCollections(int CollectionId)
        {
            return CollectionTable.Where(v => v.ParentId == CollectionId).ToArrayAsync();
        }
        public AsyncTableQuery<CollectionModel> _GetChildCollections(int parentCollectionId) => CollectionTable.Where(v => v.ParentId == parentCollectionId);

        public Task<QuestionModel[]> GetCollectionsQuestions(int CollectionId)
        {
            return QuestionTable.Where(v => v.Id.Equals(CollectionId)).ToArrayAsync();
        }

        public AsyncTableQuery<QuestionModel> _GetCollectionsQuestions(int collectionId) => QuestionTable.Where(v => v.ParentCollectionId == collectionId);
        public async Task<QuestionModel[]> GetCollectionsQuestions(int[] CollectionIds)
        {
            var quer = String.Join(",", CollectionIds);
            var list = await db.QueryAsync<QuestionModel>($"SELECT * FROM QuestionModel WHERE ParentQuestionId IN ({quer})", CollectionIds);
            return list.ToArray();
        }
        public AsyncTableQuery<QuestionModel> _GetCollectionsQuestions(int[] collectionIds) => QuestionTable.Where(v => collectionIds.Contains(v.ParentCollectionId));
        
        public async Task<AnswerModel[]> GetQuestionsAnswers(int QuestionId)
        {
            var list = await db.QueryAsync<AnswerModel>($"SELECT * FROM AnswerModel WHERE ParentQuestionId = ? ORDER BY AnswerIndex ASC", QuestionId);
            return list.ToArray();
        }
        public AsyncTableQuery<Answer> _GetQuestionsAnswers(int collectionId) => AnswerTable.Where(v => v.ParentQuestionId == collectionId);
        public Task<QuestionModel> GetQuestionAtIndex(int index)
        {
            return QuestionTable.ElementAtAsync(index);
        }
        public AsyncTableQuery<Question> _GetQuestionAtIndex(int index) => QuestionTable.ElementAtAsync(index);

        public Task<QuestionModel> GetCollectionsQuestionAtIndex(int CollectionId, int index)
        {
            return QuestionTable.Where(v => v.Id.Equals(CollectionId)).ElementAtAsync(index);
        }
        public AsyncTableQuery<Question> _GetQuestionAtIndex(AsyncTableQuery<Question> query,int index) => query.ElementAtAsync(index);

        
        public Task<List<QuestionModel>> GetQuestionsAtRange(int LimitIndex, int ResultsCount)
        {
            return db.QueryAsync<QuestionModel>($"SELECT * FROM QuestionModel ORDER BY Id ASC LIMIT ?, ?", LimitIndex, ResultsCount);
        }

        public AsyncTableQuery<QuestionModel> _GetQuestionsAtRange(int limitIndex, int resultsCount) => _GetCollectionsQuestions().OrderBy(v => v.Id).Skip(limitIndex).Take(resultsCount);
        public Task<List<QuestionModel>> GetCollectionsQuestionsAtRange(int CollectionId, int LimitIndex, int ResultsCount)
        {
            return db.QueryAsync<QuestionModel>($"SELECT * FROM QuestionModel WHERE ParentCollectionId = ? ORDER BY Id ASC LIMIT ?, ?", CollectionId, LimitIndex, ResultsCount);
        }
        public AsyncTableQuery<QuestionModel> _GetQuestionsAtRange(int limitIndex, int resultsCount, int collectionId) => _GetCollectionsQuestions(collectionId).OrderBy(v => v.Id).Skip(limitIndex).Take(resultsCount);
        public AsyncTableQuery<QuestionModel> _GetQuestionsAtRange(int limitIndex, int resultsCount, int collectionIds[]) => _GetCollectionsQuestions(collectionIds).OrderBy(v => v.Id).Skip(limitIndex).Take(resultsCount);

        public async Task<QuestionModel[]> GetCollectionsQuestionsAtRange(int[] CollectionIds, int LimitIndex, int ResultsCount)
        {
            var quer = String.Join(",", CollectionIds);
            var list = await db.QueryAsync<QuestionModel>($"SELECT * FROM QuestionModel WHERE ParentQuestionId IN ({quer}) LIMIT ? OFFSET ?", ResultsCount, LimitIndex);
            if (list.Count == 0)
            {
                throw new UnknownDatabaseErrorException("Error with querry");
            }
            return list.ToArray();
        }
        //get anything by Id
        public Task<CollectionModel> GetCollectionById(int CollectionId)
        {
            return CollectionTable.Where(v => v.Id.Equals(CollectionId)).FirstAsync();
        }
        public AsyncTableQuery<CollectionModel> _GetQuestionsAtRange(int CollectionId) => _GetCollectionsQuestions(CollectionId).OrderBy(v => v.Id).Skip(limitIndex).Take(resultsCount);

        public Task<QuestionModel> GetQuestionById(int QuestionId)
        {
            return QuestionTable.Where(v => v.Id.Equals(QuestionId)).FirstAsync();
        }
        public Task<AnswerModel> GetAnswerById(int AnswerId)
        {
            return AnswerTable.Where(v => v.Id.Equals(AnswerId)).FirstAsync();
        }
        //create
        async public Task<CollectionModel> CreateCollection(string Name)
        {
            var collection = new CollectionModel()
            {
                Name = Name
            };
            await db.InsertAsync(collection);
            return collection;
        }
        async public Task<CollectionModel> CreateCollection(CollectionModel model)
        {
            await db.InsertAsync(model);
            return model;
        }
        async public Task<QuestionModel> CreateQuestion(QuestionModel question)
        {
            await db.InsertAsync(question);
            return question;
        }
        async public Task<QuestionModel[]> CreateQuestions(QuestionModel[] questions)
        {
            await db.InsertAllAsync(questions);
            return questions;
        }
        async public Task<AnswerModel> CreateAnswer(AnswerModel answer)
        {
            await db.InsertAsync(answer);
            return answer;
        }
        async public Task<AnswerModel[]> CreateAnswers(AnswerModel[] answers)
        {
            await db.InsertAllAsync(answers);
            return answers;
        }
        //update
        async public Task UpdateQuestion(QuestionModel question)
        {
            await db.UpdateAsync((QuestionModel)question, typeof(QuestionModel));
        }
        //learing - valueable questions
        public Task<List<QuestionModel>> GetMostValuableQuestionsV1(int collectionId, int count = 10)
        {
            return db.QueryAsync<QuestionModel>($"SELECT * FROM QuestionModel WHERE ParentCollectionId = ? ORDER BY AnsweredCorrectCount - AnsweredWrongCount / 5 ASC LIMIT ?", collectionId, count);
        }
        public Task<List<QuestionModel>> GetMostValuableQuestionsV1(int count = 10)
        {
            return db.QueryAsync<QuestionModel>($"SELECT * FROM QuestionModel ORDER BY AnsweredCorrectCount - AnsweredWrongCount / 5 ASC LIMIT ?", count);
        }
        public Task<List<QuestionModel>> GetMostValuableQuestionsFromOnlyWrongsV1(int count = 10) //TODO: Rework this. Would return always the same question if answered wrong.
        {
            return db.QueryAsync<QuestionModel>($"SELECT Id FROM QuestionModel WHERE AnsweredCorrectCount = 0 AND AnsweredWrongCount > 0 ORDER BY AnsweredCorrectCount - AnsweredWrongCount / 5 ASC LIMIT {count}");
        }
        public Task<List<QuestionModel>> GetMostValuableQuestionsFromOnlyCorrectV1(int count = 10) //TODO: Rework this. Would return always the same question if answered wrong.
        {
            return db.QueryAsync<QuestionModel>($"SELECT Id FROM QuestionModel WHERE AnsweredCorrectCount > 0 ORDER BY AnsweredCorrectCount - AnsweredWrongCount / 5 ASC LIMIT {count}");
        }
        //stats based on answered
        public Task<int> GetAnsweredCorrectOnlyCount()
        {
            return QuestionTable.Where(v => v.AnsweredCorrectCount > 0 && v.AnsweredWrongCount == 0).CountAsync();
        }
        public Task<int> GetAnsweredCorrectOnlyCount(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredCorrectCount > 0 && v.AnsweredWrongCount == 0).CountAsync();
        }
        public Task<int> GetAnsweredWrongOnlyCount()
        {
            return QuestionTable.Where(v => v.AnsweredWrongCount > 0 && v.AnsweredCorrectCount == 0).CountAsync();
        }
        public Task<int> GetAnsweredWrongOnlyCount(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredWrongCount > 0 && v.AnsweredCorrectCount == 0).CountAsync();
        }
        public Task<int> GetAnsweredCorrectCount()
        {
            return QuestionTable.Where(v => v.AnsweredCorrectCount > 0).CountAsync();
        }
        public Task<int> GetAnsweredCorrectCount(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredCorrectCount > 0).CountAsync();
        }
        public Task<int> GetAnsweredWrongCount()
        {
            return QuestionTable.Where(v => v.AnsweredWrongCount > 0).CountAsync();
        }
        public Task<int> GetAnsweredWrongCount(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredWrongCount > 0).CountAsync();
        }
        public Task<int> GetAnsweredMoreCorrectThanWrongCount()
        {
            return QuestionTable.Where(v => v.AnsweredCorrectCount > v.AnsweredWrongCount).CountAsync();
        }
        public Task<int> GetAnsweredMoreCorrectThanWrongCount(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredCorrectCount > v.AnsweredWrongCount).CountAsync();
        }
        public Task<int> GetAnsweredMoreWrongThanCorrectCount()
        {
            return QuestionTable.Where(v => v.AnsweredWrongCount > v.AnsweredCorrectCount).CountAsync();
        }
        public Task<int> GetAnsweredMoreWrongThanCorrectCount(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredWrongCount > v.AnsweredCorrectCount).CountAsync();
        }
        public Task<int> GetAnsweredCount()
        {
            return QuestionTable.Where(v => v.AnsweredCorrectCount != 0 || v.AnsweredWrongCount != 0).CountAsync();
        }
        public Task<int> GetAnsweredCount(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredCorrectCount != 0 || v.AnsweredWrongCount != 0).CountAsync();
        }
        public Task<int> GetNotAnsweredCount()
        {
            return QuestionTable.Where(v => v.AnsweredCorrectCount == 0 && v.AnsweredWrongCount == 0).CountAsync();
        }
        public Task<int> GetNotAnsweredCount(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredCorrectCount == 0 && v.AnsweredWrongCount == 0).CountAsync();
        }
        //get based on answered
        public Task<QuestionModel[]> GetAnsweredCorrectOnly()
        {
            return QuestionTable.Where(v => v.AnsweredCorrectCount > 0 && v.AnsweredWrongCount == 0).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnsweredCorrectOnly(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredCorrectCount > 0 && v.AnsweredWrongCount == 0).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnsweredWrongOnly()
        {
            return QuestionTable.Where(v => v.AnsweredWrongCount > 0 && v.AnsweredCorrectCount == 0).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnsweredWrongOnly(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredWrongCount > 0 && v.AnsweredCorrectCount == 0).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnsweredCorrect()
        {
            return QuestionTable.Where(v => v.AnsweredCorrectCount > 0).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnsweredCorrect(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredCorrectCount > 0).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnsweredWrong()
        {
            return QuestionTable.Where(v => v.AnsweredWrongCount > 0).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnsweredWrong(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredWrongCount > 0).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnsweredMoreCorrectThanWrong()
        {
            return QuestionTable.Where(v => v.AnsweredCorrectCount > v.AnsweredWrongCount).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnsweredMoreCorrectThanWrong(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredCorrectCount > v.AnsweredWrongCount).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnsweredMoreWrongThanCorrect()
        {
            return QuestionTable.Where(v => v.AnsweredWrongCount > v.AnsweredCorrectCount).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnsweredMoreWrongThanCorrect(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredWrongCount > v.AnsweredCorrectCount).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnswered()
        {
            return QuestionTable.Where(v => v.AnsweredCorrectCount != 0 || v.AnsweredWrongCount != 0).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetAnswered(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredCorrectCount != 0 || v.AnsweredWrongCount != 0).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetNotAnswered()
        {
            return QuestionTable.Where(v => v.AnsweredCorrectCount == 0 && v.AnsweredWrongCount == 0).ToArrayAsync();
        }
        public Task<QuestionModel[]> GetNotAnswered(int CollectionId)
        {
            return QuestionTable.Where(v => v.ParentCollectionId.Equals(CollectionId) && v.AnsweredCorrectCount == 0 && v.AnsweredWrongCount == 0).ToArrayAsync();
        }

        
    }

}
