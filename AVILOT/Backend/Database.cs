using AVILOT.Backend.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace AVILOT.Backend
{
    public class Database
    {
        readonly SQLiteAsyncConnection con;
        readonly string path;

        public Database(string dbPath)
        {
            con = new SQLiteAsyncConnection(dbPath);
            path = dbPath;

        }
        // must be called before running
        public async Task InitializeAsync()
        {
            await createTables();
        }

        // misc
        private async Task createTables()
        {
            Console.WriteLine("create tables");
            Stopwatch sw = Stopwatch.StartNew();
            await Task.WhenAll(
                con.CreateTablesAsync<Category, TestTemplate, Template_Question, Question, Answer>(),
                con.CreateTablesAsync<Test, TestQuestion, PracticeAnswer, QuestionBookmark>(SQLite.CreateFlags.AutoIncPK)
            );
            sw.Stop();
            Console.WriteLine($"created tables in {sw.ElapsedMilliseconds}");

            // old
            /*await con.CreateTableAsync<Category>();
            await con.CreateTableAsync<TestTemplate>();
            await con.CreateTableAsync<Template_Question>();
            await con.CreateTableAsync<Question>();
            await con.CreateTableAsync<Answer>();
            await con.CreateTableAsync<Test>(SQLite.CreateFlags.AutoIncPK);
            await con.CreateTableAsync<TestQuestion>(SQLite.CreateFlags.AutoIncPK);
            await con.CreateTableAsync<PracticeAnswer>(SQLite.CreateFlags.AutoIncPK);
            await con.CreateTableAsync<QuestionBookmark>(SQLite.CreateFlags.AutoIncPK);*/
        }

        public async Task updateDataset(string filepath)
        {
            DatasetLoader loader = new DatasetLoader(con);
            await loader.updateDataset(filepath);
        }
        public async Task updateDataset(System.IO.Stream stream, string filename)
        {
            DatasetLoader loader = new DatasetLoader(con);
            await loader.updateDataset(stream, filename);
        }

        // queries
        public async Task<List<Category>> getCategories()
        {
            return await con.Table<Category>().ToListAsync();
        }

        public async Task<List<TestTemplate>> getTestTemplates(Category category)
        {
            var category_id = category.category_id;
            return await con.Table<TestTemplate>().Where(tt => tt.category == category_id).ToListAsync();
        }
        public async Task<TestTemplate> getTestTestTemplate(Test test)
        {
            return await con.Table<TestTemplate>().Where(tt => tt.template_id == test.template).FirstOrDefaultAsync();
        }
        public async Task<List<Test>> getActiveTests(Category category)
        {
            return await con.QueryAsync<Test>(
                @"SELECT Test.* FROM Category
                LEFT JOIN TestTemplate ON
                    Category.category_id = TestTemplate.category
                INNER JOIN Test ON
                    TestTemplate.template_id = Test.template
                WHERE Category.category_id = ?;", category.category_id);
        }
        public async Task<List<Test>> getActiveTests(TestTemplate testTemplate)
        {
            return await con.Table<Test>().Where(t => (t.template == testTemplate.template_id) && (t.end_time == null)).ToListAsync();
        }
        

        public async Task<float> getBestScore(TestTemplate testTemplate)
        {
            throw new NotImplementedException();
        }
        public async Task fillTestWithQuestions(Test test)
        {
            List<string> questionIds = await con.QueryScalarsAsync<string>(@"
                SELECT Question.question_id FROM Test
                LEFT JOIN TestTemplate ON
                    Test.template = TestTemplate.template_id
                LEFT JOIN Template_Question ON
                    TestTemplate.template_id = Template_Question.template
                LEFT JOIN Question ON
                    Template_Question.question = Question.question_id
                WHERE Test.test_id = ?
                ORDER BY RANDOM()
                LIMIT (
                    SELECT TestTemplate.question_count FROM Test
                    LEFT JOIN TestTemplate ON
                        Test.template = TestTemplate.template_id
                    WHERE Test.test_id = ?
                )
                ", test.test_id, test.test_id);
            List<TestQuestion> testQuestions = questionIds.Select(
                    id => new TestQuestion()
                    {
                        question = id,
                        test = test.test_id ?? 0
                    }
                ).ToList();
            await con.InsertAllAsync(testQuestions);
        }
        public async Task<Test> startTest(TestTemplate testTemplate)
        {
            Test test = new Test();
            test.template = testTemplate.template_id;
            test.start_time = DateTime.Now;
            test.end_time = null;
            await con.InsertAsync(test);
            await fillTestWithQuestions(test);
            return test;
        }

        public async Task<List<Question>> getTestQuestions(Test test)
        {
            return await con.QueryAsync<Question>(@"
                SELECT Question.* FROM Test
                LEFT JOIN TestQuestion ON
                    Test.test_id = TestQuestion.test
                LEFT JOIN Question ON
                    TestQuestion.question = Question.question_id
                WHERE Test.test_id = ?
                ORDER BY TestQuestion.id
                ", test.test_id);
        }
        public async Task<List<Answer>> getAnswers(Question question)
        {
            return await con.Table<Answer>().Where(a => a.question == question.question_id).ToListAsync();
        }

        public async Task<PracticeAnswer> answerTestQuestion(Answer answer, Test test)
        {
            await con.ExecuteAsync(@"
            INSERT OR REPLACE INTO PracticeAnswer (row_id, answered_time, answer, test)
            VALUES (
                COALESCE((SELECT row_id FROM PracticeAnswer WHERE answer = ? AND test = ?), NULL),
                ?,
                ?,
                ?
            );
            ", answer.answer_id, test.test_id, DateTime.Now, answer.answer_id, test.test_id);
            return await con.Table<PracticeAnswer>().Where(pa => (pa.answer == answer.answer_id) && (pa.test == test.test_id)).FirstAsync();
        }

        public async Task<PracticeAnswer> answerPracticeQuestion(Answer answer)
        {
            PracticeAnswer record = new PracticeAnswer()
            {
                answer = answer.answer_id,
                answered_time = DateTime.Now,
                test = null
            };
            await con.InsertAsync(record);
            return record;
        }
    }



}

