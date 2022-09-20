using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Diagnostics;


namespace AVILOT.AVQuestionsEngine
{
    public static class QuestionsEngine
    {
        private static Database.QuestionsDatabase _GlobalQuestionsDatabase;
        public static Database.QuestionsDatabase GlobalQuestionsDatabase
        {
            get
            {
                if (! initialized)
                {
                    throw new QuestionEngineNotInitializedException();
                }
                return _GlobalQuestionsDatabase;
            }
        }
        private static AllQuestionsCollection _allQuestionsCollection;
        public static AllQuestionsCollection allQuestionsCollection
        {
            get
            {
                if (!initialized)
                {
                    throw new QuestionEngineNotInitializedException();
                }
                return _allQuestionsCollection;
            }
        }

        static QuestionsEngine()
        {
            initialized = false;
        }
        private static bool initialized;
        public static async Task Initialize()
        {
            Debug.WriteLine("Initializing Questions Engine");
            _GlobalQuestionsDatabase = await Database.DbUtils.GetMainDatabase();
            _allQuestionsCollection = new AllQuestionsCollection(_GlobalQuestionsDatabase);
            Debug.WriteLine("Initialized Qestions Engine");
            initialized = true;
        }
        public static async Task<QuestionsCollection[]> GetAllColections()
        {
            var db = GlobalQuestionsDatabase;
            var collections = await GlobalQuestionsDatabase.GetCollections();
            return await Task.WhenAll( collections.Select(async x => await QuestionsCollection.fromModel(x, db)));
        }
        public static async Task<QuestionsCollection> GetCollectionById(int id)
        {
            var db = GlobalQuestionsDatabase;
            var model = await GlobalQuestionsDatabase.GetCollectionById(id);
            return await QuestionsCollection.fromModel(model, db);
        }
        #region ImportQuestionsFromCSV
        
        public static Task<QuestionsCollection> ImportQuestionsFromCsv(StreamReader reader, string filename, string CollectionName = null)
        {
            var (questionModels, answerModels, collectionModel) = Database.CsvUtils.LoadQuestionsFromCSV(reader, filename, CollectionName); //Load question models from CSV
            return ImportQuestionsToDatabase(collectionModel, questionModels, answerModels);
        }
        #endregion
        public static async Task<QuestionsCollection> ImportQuestionsToDatabase(Database.CollectionModel collectionModel_, Database.QuestionModel[] questionModels, Database.AnswerModel[][] answerModels)
        {
            var collectionModel = await GlobalQuestionsDatabase.CreateCollection(collectionModel_); //create collection in db
            //set correct parentCollectionId for each question
            var collectionId = collectionModel.Id;
            foreach (var questionModel in questionModels)
            {
                questionModel.ParentCollectionId = collectionId;
            }
            var questionModelsWithIds = await GlobalQuestionsDatabase.CreateQuestions(questionModels); //create questions id db
            //set correct answers parentQuestionIds
            var answersCount = answerModels.Length * answerModels[0].Length;
            var readyAnswersIndex = 0;
            var readyAnswerModels = new Database.AnswerModel[answersCount]; //this is here just to flatten the array
            for (int i = 0; i < questionModelsWithIds.Length; i++)
            {
                var questionModelId = questionModelsWithIds[i].Id; //get id of parent question
                var answerModelz = answerModels[i]; // get answer models asigned to the question
                foreach (var answerModel in answerModelz)
                {
                    answerModel.ParentQuestionId = questionModelId;
                    //add answerModel to readyAnswerModels
                    readyAnswerModels[readyAnswersIndex] = answerModel;
                    readyAnswersIndex++;
                }
            }
            await GlobalQuestionsDatabase.CreateAnswers(readyAnswerModels);
            return await GetCollectionById(collectionId);
        }

    }
    public class Answer : Database.AnswerModel
    {
        public Question ParentQuestion { get; set; }

        public Task<bool> Select()
        {
            return ParentQuestion.Select(this);
        }

        internal static Answer FromModel(Database.AnswerModel model, Question parent)
        {
            return new Answer()
            {
                Id = model.Id,
                Text = model.Text,
                Correct = model.Correct,
                ParentQuestionId = model.ParentQuestionId,
                ParentQuestion = parent,
                AnsweredExplanation = model.AnsweredExplanation,
                AnswerIndex = model.AnswerIndex,
                ImageUrl = model.ImageUrl,
            };
            
        }
    }
    public class Question : Database.QuestionModel
    {
        public Answer[] Answers { get; set; }
        public QuestionsCollection ParentCollection { get; set; }

        public Task<bool> Select(Answer answer)
        {
            return ParentCollection.AnswerQuestion(this, answer.Correct);
        }
        public Task<bool> Select(int index)
        {
            return ParentCollection.AnswerQuestion(this, index == CorrectAnswerIndex);
        }
        
        internal static Question fromModel(Database.QuestionModel model, Database.AnswerModel[] answerModels, QuestionsCollection parent)
        {
            var question = new Question() //transfer all data
            {
                Id = model.Id,
                Text = model.Text,
                ParentCollectionId = model.ParentCollectionId,
                CorrectAnswerIndex = model.CorrectAnswerIndex,
                LastCorrectAnswer = model.LastCorrectAnswer,
                LastWrongAnswer = model.LastWrongAnswer,
                AnsweredCorrectCount = model.AnsweredCorrectCount,
                AnsweredWrongCount = model.AnsweredWrongCount,
                ImageUrl = model.ImageUrl
            };
            question.ParentCollection = parent; //set parent collection
            question.Answers = new Answer[answerModels.Length];
            for (int i = 0; i < answerModels.Length; i++)
            {
                question.Answers[i] = Answer.FromModel(answerModels[i], question);
            }
            return question;
        }
    }

    public class UnknownDatabaseErrorException : Exception
    {
        public UnknownDatabaseErrorException(string msg) : base(msg) { }
    }
    public class QuestionsCollection : Database.CollectionModel
    {
        //init
        public readonly Database.QuestionsDatabase database;
        public QuestionsCollection[] ChildCollections { get; private set; }

        public QuestionsCollection(Database.QuestionsDatabase db)
        {
            database = db;
        }
        internal async static Task<QuestionsCollection> fromModel(Database.CollectionModel model, Database.QuestionsDatabase db)
        {
            var collection = new QuestionsCollection(db)
            {
                Id = model.Id,
                Name = model.Name,
                About = model.About,
                ImageBannerUrl = model.ImageBannerUrl,
                ImageUrl = model.ImageUrl,
                ParentId = model.ParentId
            };
            var childCols = await db.GetSubCollections(model.Id);
            if (childCols != null)
            {
                collection.ChildCollections = await Task.WhenAll(childCols.Select(async i => await fromModel(i, db)));
            }
            return collection;

        }
        //getting questions - ALL
        public async virtual Task<Question[]> GetAllQuestions()
        {
            var models = await database.GetCollectionsQuestions(Id);
            return await QuestionsFromModels(models);
        }
        public virtual async IAsyncEnumerable<Question> IterateOverQuestionsAsync()
        {
            var questionCount = await GetQuestionCount();
            var currentQuestionIndex = 0;
            while (currentQuestionIndex < questionCount)
            {
                var questionModel = await database.GetCollectionsQuestionAtIndex(Id, currentQuestionIndex);
                yield return await QuestionFromModel(questionModel);
                currentQuestionIndex++;
            }
        }
        public virtual async IAsyncEnumerable<Question[]> IterateOverQuestionPagesAsync(int PerPageCount)
        {
            var questionCount = await GetQuestionCount();
            var currentQuestionIndex = 0;
            do
            {
                var questionModels = await database.GetCollectionsQuestionsAtRange(Id, currentQuestionIndex, PerPageCount);
                yield return await QuestionsFromModels(questionModels.ToArray());
                currentQuestionIndex += PerPageCount;
            }
            while (currentQuestionIndex < questionCount);
        }
        //getting questions
        public virtual async Task<Question> GetMostValuableQuestionAsync()
        {
            var models = await database.GetMostValuableQuestionsV1(this.Id, 1);
            if (models == null || models.Count == 0)
            {
                throw new UnknownDatabaseErrorException("Most valuable question retuned zero questions");
            }
            return await QuestionFromModel(models[0]);
        }
        
        //answer questions
        public Task<bool> AnswerQuestion(Question question, int answerIndex)
        {
            var correctAnswer = (question.CorrectAnswerIndex == answerIndex);
            return AnswerQuestion(question, correctAnswer);
        }
        public async Task<bool> AnswerQuestion(Question question, bool correctAnswer)
        {
            if (correctAnswer)
            {
                question.LastCorrectAnswer = DateTime.Now;
                question.AnsweredCorrectCount += 1;
            }
            else
            {
                question.LastWrongAnswer = DateTime.Now;
                question.AnsweredWrongCount += 1;
            }
            await database.UpdateQuestion(question);
            return correctAnswer;
        }
        //stats
        public virtual Task<int> GetQuestionCount()
        {
            return database.GetQuestionsCount(this.Id);
        }
        public async Task<double> GetAnsweredCorrectPercentage()
        {
            return (await GetAnsweredCorrectCount()) / (await GetQuestionCount()) * 100;
        }
        public virtual async Task<Question> GetQuestion(int QuestionId)
        {
            var questionModel = await database.GetQuestionById(QuestionId);
            if (questionModel.ParentCollectionId != this.Id)
            {
                throw new InvalidOperationException($"Question with id {QuestionId} does not belong to collection '{this.Name}'");
            }
            return await QuestionFromModel(questionModel);

        }
        //utils
        protected async Task<Question> QuestionFromModel(Database.QuestionModel questionModel)
        {
            var answerModels = await database.GetQuestionsAnswers(questionModel.Id);
            return Question.fromModel(questionModel, answerModels, this);
        }
        protected async Task<Question[]> QuestionsFromModels(Database.QuestionModel[] questionModels)
        {
            var questions = new Question[questionModels.Length];
            for (int i = 0; i < questionModels.Length; i++)
            {
                var questionModel = questionModels[i];
                var answerModels = await database.GetQuestionsAnswers(questionModel.Id);
                questions[i] = Question.fromModel(questionModel, answerModels, this);
            }
            return questions;
        }
        #region GetAnswered
        public virtual async Task<Database.QuestionModel[]> GetAnsweredCorrectOnly()
        {
            var models = await database.GetAnsweredCorrectOnly(this.Id);
            return await QuestionsFromModels(models);
        }
        public virtual async Task<Database.QuestionModel[]> GetAnsweredWrongOnly()
        {
            var models = await database.GetAnsweredWrongOnly(this.Id);
            return await QuestionsFromModels(models);
        }
        public virtual async Task<Database.QuestionModel[]> GetAnsweredCorrect()
        {
            var models = await database.GetAnsweredCorrect(this.Id);
            return await QuestionsFromModels(models);
        }
        public virtual async Task<Database.QuestionModel[]> GetAnsweredWrong()
        {
            var models = await database.GetAnsweredWrong(this.Id);
            return await QuestionsFromModels(models);
        }
        public virtual async Task<Database.QuestionModel[]> GetAnsweredMoreCorrectThanWrong()
        {
            var models = await database.GetAnsweredMoreCorrectThanWrong(this.Id);
            return await QuestionsFromModels(models);
        }
        public virtual async Task<Database.QuestionModel[]> GetAnsweredMoreWrongThanCorrect()
        {
            var models = await database.GetAnsweredMoreWrongThanCorrect(this.Id);
            return await QuestionsFromModels(models);
        }
        public virtual async Task<Database.QuestionModel[]> GetAnswered()
        {
            var models = await database.GetAnswered(this.Id);
            return await QuestionsFromModels(models);
        }
        public virtual async Task<Database.QuestionModel[]> GetNotAnswered()
        {
            var models = await database.GetNotAnswered(this.Id);
            return await QuestionsFromModels(models);
        }
        #endregion
        #region GetAnsweredCount
        public virtual Task<int> GetAnsweredCorrectOnlyCount() { return database.GetAnsweredCorrectOnlyCount(this.Id); }
        public virtual Task<int> GetAnsweredWrongOnlyCount() { return database.GetAnsweredWrongOnlyCount(this.Id); }
        public virtual Task<int> GetAnsweredCorrectCount() { return database.GetAnsweredCorrectCount(this.Id); }
        public virtual Task<int> GetAnsweredWrongCount() { return database.GetAnsweredWrongCount(this.Id); }
        public virtual Task<int> GetAnsweredMoreCorrectThanWrongCount() { return database.GetAnsweredMoreCorrectThanWrongCount(this.Id); }
        public virtual Task<int> GetAnsweredMoreWrongThanCorrectCount() { return database.GetAnsweredMoreWrongThanCorrectCount(this.Id); }
        public virtual Task<int> GetAnsweredCount() { return database.GetAnsweredCount(this.Id); }
        public virtual Task<int> GetNotAnsweredCount() { return database.GetNotAnsweredCount(this.Id); }
        #endregion
    }

    //HERE IS ALL QUESTIONS !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public class AllQuestionsCollection : QuestionsCollection
    {
        public AllQuestionsCollection(Database.QuestionsDatabase db) : base(db) { }
        public override async Task<Question[]> GetAllQuestions()
        {
            var models = await database.GetQuestions();
            return await QuestionsFromModels(models);
        }
        public override async IAsyncEnumerable<Question> IterateOverQuestionsAsync()
        {
            var questionCount = await GetQuestionCount();
            var currentQuestionIndex = 0;
            while (currentQuestionIndex < questionCount)
            {
                var questionModel = await database.GetQuestionAtIndex(currentQuestionIndex);
                yield return await QuestionFromModel(questionModel);
                currentQuestionIndex++;
            }
        }
        public override async IAsyncEnumerable<Question[]> IterateOverQuestionPagesAsync(int PerPageCount)
        {
            var questionCount = await GetQuestionCount();
            var currentQuestionIndex = 0;
            do
            {
                var questionModels = await database.GetQuestionsAtRange(currentQuestionIndex, PerPageCount);
                yield return await QuestionsFromModels(questionModels.ToArray());
                currentQuestionIndex += PerPageCount;
            }
            while (currentQuestionIndex < questionCount);
        }
        //getting questions
        public override async Task<Question> GetMostValuableQuestionAsync()
        {
            var model = (await database.GetMostValuableQuestionsV1(1))[0];
            return await QuestionFromModel(model);
        }

        //stats
        public override Task<int> GetQuestionCount()
        {
            return database.GetQuestionsCount();
        }
        public override async Task<Question> GetQuestion(int QuestionId)
        {
            var questionModel = await database.GetQuestionById(QuestionId);
            return await QuestionFromModel(questionModel);

        }
        #region GetAnswered
        public override async Task<Database.QuestionModel[]> GetAnsweredCorrectOnly()
        {
            var models = await database.GetAnsweredCorrectOnly();
            return await QuestionsFromModels(models);
        }
        public override async Task<Database.QuestionModel[]> GetAnsweredWrongOnly()
        {
            var models = await database.GetAnsweredWrongOnly();
            return await QuestionsFromModels(models);
        }
        public override async Task<Database.QuestionModel[]> GetAnsweredCorrect()
        {
            var models = await database.GetAnsweredCorrect();
            return await QuestionsFromModels(models);
        }
        public override async Task<Database.QuestionModel[]> GetAnsweredWrong()
        {
            var models = await database.GetAnsweredWrong();
            return await QuestionsFromModels(models);
        }
        public override async Task<Database.QuestionModel[]> GetAnsweredMoreCorrectThanWrong()
        {
            var models = await database.GetAnsweredMoreCorrectThanWrong();
            return await QuestionsFromModels(models);
        }
        public override async Task<Database.QuestionModel[]> GetAnsweredMoreWrongThanCorrect()
        {
            var models = await database.GetAnsweredMoreWrongThanCorrect();
            return await QuestionsFromModels(models);
        }
        public override async Task<Database.QuestionModel[]> GetAnswered()
        {
            var models = await database.GetAnswered();
            return await QuestionsFromModels(models);
        }
        public override async Task<Database.QuestionModel[]> GetNotAnswered()
        {
            var models = await database.GetNotAnswered();
            return await QuestionsFromModels(models);
        }
        #endregion
        #region GetAnsweredCount
        public override Task<int> GetAnsweredCorrectOnlyCount() { return database.GetAnsweredCorrectOnlyCount(); }
        public override Task<int> GetAnsweredWrongOnlyCount() { return database.GetAnsweredWrongOnlyCount(); }
        public override Task<int> GetAnsweredCorrectCount() { return database.GetAnsweredCorrectCount(); }
        public override Task<int> GetAnsweredWrongCount() { return database.GetAnsweredWrongCount(); }
        public override Task<int> GetAnsweredMoreCorrectThanWrongCount() { return database.GetAnsweredMoreCorrectThanWrongCount(); }
        public override Task<int> GetAnsweredMoreWrongThanCorrectCount() { return database.GetAnsweredMoreWrongThanCorrectCount(); }
        public override Task<int> GetAnsweredCount() { return database.GetAnsweredCount(); }
        public override Task<int> GetNotAnsweredCount() { return database.GetNotAnsweredCount(); }
        #endregion
    }
    public class JoinedQuestionsCollection : QuestionsCollection
    {
        public readonly QuestionsCollection[] JoinedCollections;
        public readonly int[] JoinedCollectionsIds;
        public JoinedQuestionsCollection(QuestionsCollection[] collections) : base(collections[0].database)
        {
            if (collections == null || collections.Length == 0)
            {
                throw new ArgumentNullException(nameof(collections));
            }
            JoinedCollections = collections;
            JoinedCollectionsIds = collections.Select(x => x.Id).ToArray();

        }
        public override async Task<Question[]> GetAllQuestions()
        {
            var models = await database.GetCollectionsQuestions(JoinedCollectionsIds);
            return await QuestionsFromModels(models);
        }
        public override async IAsyncEnumerable<Question> IterateOverQuestionsAsync()
        {
            var questionCount = await GetQuestionCount();
            var currentQuestionIndex = 0;
            const int PerPageCount = 0;
            do
            {
                var questionModels = await database.GetCollectionsQuestionsAtRange(JoinedCollectionsIds, currentQuestionIndex, PerPageCount);
                yield return await QuestionFromModel(questionModels[0]);
                currentQuestionIndex += PerPageCount;
            }
            while (currentQuestionIndex < questionCount);
        }
        public override async IAsyncEnumerable<Question[]> IterateOverQuestionPagesAsync(int PerPageCount)
        {
            var questionCount = await GetQuestionCount();
            var currentQuestionIndex = 0;
            do
            {
                var questionModels = await database.GetCollectionsQuestionsAtRange(JoinedCollectionsIds, currentQuestionIndex, PerPageCount);
                yield return await QuestionsFromModels(questionModels);
                currentQuestionIndex += PerPageCount;
            }
            while (currentQuestionIndex < questionCount);
        }

    }
}
