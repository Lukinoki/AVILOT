using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVILOT.QEngine2
{
    public static class Engine
    {
        private static QeEngine2 _engine;
        public static QeEngine2 engine
        {
            get
            {
                if (_engine == null)
                {
                    throw new InvalidOperationException("Question Engine is not initialized, you have to call Init()");
                }
                return _engine;
            }
        }

        public static async Task Init()
        {
            _engine = new QeEngine2(Utils.GetMainDatabase());
            await engine.createAllModels();
        }

    }


    public class QeEngine2
    {
        private SQLiteAsyncConnection db;
        public QeEngine2(string dbPath)
        {
            db = new SQLiteAsyncConnection(dbPath);
            createAllModels();
        }
        public QeEngine2(SQLiteAsyncConnection connection)
        {
            this.db = connection;
        }
        public async Task createAllModels()
        {
            await db.CreateTableAsync<Answer>();
            await db.CreateTableAsync<Question>();
            await db.CreateTableAsync<Collection>();
            await db.CreateTableAsync<Category>();
            await db.CreateTableAsync<TestQuestion>();
            await db.CreateTableAsync<Test>();
        }
        public async Task clearDatabase()
        {
            await db.DeleteAllAsync<Answer>();
            await db.DeleteAllAsync<Question>();
            await db.DeleteAllAsync<Collection>();
            await db.DeleteAllAsync<Category>();
            await db.DeleteAllAsync<Test>();
            await db.DeleteAllAsync<TestQuestion>();
        }

        public async Task ImportJsonCollection(string path, string CategoryId)
        {

        }

        public async Task CreateCategory()
        {

        }


        public Task<List<Category>> GetCategories() {
            return DBModel.Activate(db.Table<Category>().ToListAsync(), db);
        }

        public Task<List<Test>> GetTests()
        {
            return DBModel.Activate(db.Table<Test>().ToListAsync(), db);
        }
           
    }
    public class DBModel
    {
        private SQLiteAsyncConnection _db;
        [Ignore]
        protected SQLiteAsyncConnection db
        {
            get
            {
                if (_db == null)
                {
                    throw new InvalidOperationException($"{this.GetType().Name} has not been given reference to db. Use InitDb(db) to init");
                }
                return _db;
            }
        }
        public void InitDB(SQLiteAsyncConnection db)
        {
            this._db = db;
        }

        public static List<T> Activate<T>(List<T> models, SQLiteAsyncConnection db) where T : DBModel
        {
            models.ForEach(x => x.InitDB(db));
            return models;
        }

        public static T Activate<T>(T model, SQLiteAsyncConnection db) where T : DBModel
        {
            model.InitDB(db);
            return model;
        }

        public async static Task<List<T>> Activate<T>(Task<List<T>> models,  SQLiteAsyncConnection db) where T : DBModel
        {
            var mods = await models;
            mods.ForEach(x => x.InitDB(db));
            return mods;
        }
    }

    [SQLite.Table("Answer")]
    public class Answer : DBModel
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Text { get; set; }
        public string ImageUrl { get; set; }
        public string AnsweredExplanation { get; set; }
        public bool Correct { get; set; }
        public int AnswerIndex { get; set; }

        [Indexed, ForeignKey(typeof(Question))]
        public int ParentQuestionId { get; set; }
        
        [ManyToOne]
        public Question _ParentQuestion { get; set; }

        #endregion

        [Ignore]
        public Question ParentQuestion { get
            {
                _ParentQuestion.InitDB(this.db);
                return _ParentQuestion;
            } }
    }


    [SQLite.Table("Question")]
    public class Question : DBModel
    {
        #region Properties
        //database id
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        //main info
        public string Text { get; set; }

        public string ImageUrl { get; set; }
        [Indexed, ForeignKey(typeof(Collection))]
        public int? ParentCollectionId { get; set; }
        [ManyToOne]
        public Category _ParentCollection { get; set; }
     
        public int CorrectAnswerIndex { get; set; }
        
        [OneToMany]
        public List<Answer> _Answers { get; set; }
        #endregion
        [Ignore]
        public List<Answer> Answers
        {
            get
            {
                _Answers.ForEach(a=> a.InitDB(this.db));
                return _Answers;
            }
        }
        [Ignore]
        public Category ParentCollection
        {
            get
            {
                _ParentCollection.InitDB(this.db);
                return _ParentCollection;
            }
        }


    }
    
    
    [SQLite.Table("Collection")]
    public class Collection : DBModel
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public bool Downloaded { get; set; }
        public bool OfflineMedia { get; set; }

        public string Name { get; set; }
        public string About { get; set; }
        public string ImageUrl { get; set; }
        public string ImageBannerUrl { get; set; }
        [ForeignKey(typeof(Category))]
        public int? ParentCollectionId { get; set; }

        [ManyToOne]
        public Question _ParentQuestion { get; set; }
           

        [OneToMany]
        public List<Question> _Questions { get; set; }
        #endregion
        [Ignore]
        public List<Question> Questions
        {
            get
            {
                _Questions.ForEach(q=> q.InitDB(this.db));
                return _Questions;
            }
        }
        [Ignore]
        public Question ParentQuestion { get
            {
                _ParentQuestion.InitDB(this.db);
                return _ParentQuestion;
            } }

    }


    [SQLite.Table("Category")]
    public class Category : DBModel
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public string ImageUrl { get; set; }
        public string ImageBannerUrl { get; set; }
        [OneToMany]
        public List<Collection> _SubCollections { get; set; }

        #endregion
        [Ignore]
        public List<Collection> SubCollections
        {
            get
            {
                _SubCollections.ForEach(s => s.InitDB(this.db));
                return _SubCollections;
            }
        }
    }


    [SQLite.Table("TestQuestion")]
    public class TestQuestion : DBModel
    {
        #region Properties
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed, ForeignKey(typeof(Question))]
        public int ParentQuestionId { get; set; }
        [OneToOne]
        public Question _ParentQuestion { get; set; }
        
        [Indexed, ForeignKey(typeof(Test))]
        public int ParentTestId { get; set; }
        [OneToOne]
        public Test _ParentTest { get; set; }
        #endregion
        public Question ParentQuestion
        {
            get
            {
                _ParentQuestion.InitDB(db);
                return ParentQuestion;
            }
        }
        public Test ParentTest { get
            {
                _ParentTest.InitDB(db);
                return ParentTest;
            } }
        
        //stats
        public DateTime? LastCorrectAnswer { get; set; }
        public DateTime? LastWrongAnswer { get; set; }
        public int AnsweredCorrectCount { get; set; }
        public int AnsweredWrongCount { get; set; }


        public TestQuestion(Question parentQuestion, Test parentTest)
        {
            this.ParentQuestionId = parentQuestion.Id;
            this.ParentTestId = parentTest.Id;
        }
        public TestQuestion() { }

        public async Task<bool> AnswerQuestion(Answer answer)
        {
            if (answer.ParentQuestionId != this.ParentQuestionId)
            {
                throw new InvalidOperationException($"Incompatible answer, testQuestionId={this.Id}, ParentQuestionId={this.ParentQuestionId}, answersParentId={answer.ParentQuestionId}");
            }
            
            if (answer.Correct)
            {
                LastCorrectAnswer = DateTime.UtcNow;
                AnsweredCorrectCount += 1;
            }
            else
            {
                LastWrongAnswer = DateTime.UtcNow;
                AnsweredWrongCount += 1;
            }
            await db.UpdateAsync(this);
            return answer.Correct;
        }
    }


    [SQLite.Table("Test")]
    public class Test : DBModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public string DerivedFrom { get; set; }

        [OneToMany]
        public List<TestQuestion> _TestQuestions { get; set; }

        public List<TestQuestion> TestQuestions { get
            {
                return DBModel.Activate(_TestQuestions, db);
            } }

        public static async Task<Test> createTest(SQLiteAsyncConnection db, string name, string type)
        {
            var test = new Test()
            {
                Name = name,
                Type = type
            };
            DBModel.Activate(test, db);
            await db.InsertAsync(test);
            return test;
        }
        public Task<List<TestQuestion>> addQuestions(Collection collection)
        {
            return addQuestions(collection.Questions);
        }

        public async Task<List<TestQuestion>> addQuestions(List<Question> questions)
        {
            var tquestions = questions.Select(q =>
             {
                 return DBModel.Activate(new TestQuestion(q, this), db);
             });
            await db.InsertAllAsync(tquestions);
            return tquestions.ToList();
        }

        private AsyncTableQuery<TestQuestion> ownQuestions()
        {
            return db.Table<TestQuestion>().Where(t => t.ParentTestId == this.Id);
        }
        public Task<int> getCount()
        {
            return ownQuestions().CountAsync();
        }

        public Task<List<TestQuestion>> getAnswered()
        {
            return DBModel.Activate(ownQuestions().Where(t=> (t.AnsweredCorrectCount > 0) | (t.AnsweredWrongCount > 0)).ToListAsync(), db);
        }

        public Task<int> getAnsweredCount()
        {
            return ownQuestions().Where(t => (t.AnsweredCorrectCount > 0) | (t.AnsweredWrongCount > 0)).CountAsync();
        }

        public Task<List<TestQuestion>> getNotAnswered()
        {
            return DBModel.Activate(ownQuestions().Where(t => (t.AnsweredCorrectCount == 0) & (t.AnsweredWrongCount == 0)).ToListAsync(), db);
        }
        public Task<int> getNotAnsweredCount()
        {
            return ownQuestions().Where(t => (t.AnsweredCorrectCount == 0) & (t.AnsweredWrongCount == 0)).CountAsync();
        }

    }
}
