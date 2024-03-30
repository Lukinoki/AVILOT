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

        public SQLiteAsyncConnection debugDatabaseGetAsyncSQLiteConnection()
        {
            return con;
        }


        public Database(string dbPath)
        {
            con = new SQLiteAsyncConnection(dbPath);
            path = dbPath;
            Console.WriteLine("created db at " + con.DatabasePath);

        }
        // must be called before running
        public async Task InitializeAsync()
        {
            await createTables();
        }

        // misc
        private async Task createTables()
        {
            
            await Task.WhenAll(
                con.CreateTablesAsync<Category, TestTemplate, Template_Question, Question>(),
                con.CreateTablesAsync<Answer, Media>(),
                con.CreateTablesAsync<Test, TestQuestion, PracticeAnswer, QuestionBookmark>(SQLite.CreateFlags.AutoIncPK)
            );

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

        public async Task<Category> getCategoryById(string id)
        {
            return await con.Table<Category>().Where(c => c.category_id == id).FirstOrDefaultAsync();
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
                    COALESCE(
                        (SELECT TestTemplate.question_count FROM Test
                        LEFT JOIN TestTemplate ON
                            Test.template = TestTemplate.template_id
                        WHERE Test.test_id = ?),
                        (SELECT COUNT(*) as cnt FROM Question)
                    )
                )
                ", test.test_id, test.test_id);
            Console.WriteLine(String.Join(", ", questionIds));
            Console.WriteLine(questionIds.Count);
            if (questionIds.Count == 0 || questionIds[0] == null)
            {
                throw new Exception($"No questions found for testemplate: {test.template}");
            }
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
        public async Task endTest(Test test)
        {
            test.end_time = DateTime.Now;
            await con.UpdateAsync(test);
        }

        public async Task<List<Question>> getTestQuestions(Test test)
        {
            return await con.QueryAsync<Question>(@"
                SELECT Question.* FROM TestQuestion
                INNER JOIN Question ON TestQuestion.question = Question.question_id
                WHERE TestQuestion.test = ?
                ", test.test_id);
        }
        public async Task<List<Answer>> getAnswers(Question question)
        {
            return await con.Table<Answer>().Where(a => a.question == question.question_id).ToListAsync();
        }

        public async Task<PracticeAnswer> answerTestQuestion(Answer answer, Test test)
        {
            var changed = await con.ExecuteAsync(@"
            INSERT OR REPLACE INTO PracticeAnswer (row_id, answered_time, answer, test)
            VALUES (
                (SELECT pa.row_id FROM
                    (SELECT * FROM PracticeAnswer
                    WHERE PracticeAnswer.answer = ? AND PracticeAnswer.test = ?) AS op
                LEFT JOIN Answer a ON op.answer = a.answer_id
                LEFT JOIN Question q ON a.question = q.question_id
                LEFT JOIN Answer aa ON q.question_id = aa.question
                INNER JOIN PracticeAnswer pa ON aa.answer_id = pa.answer AND pa.test = op.test),
                ?,
                ?,
                ?
            );
            ", answer.answer_id, test.test_id, DateTime.Now, answer.answer_id, test.test_id);
            Console.WriteLine($"insering {answer.answer_id} answer changed: {changed} rows");
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

        public async Task<Question> getPracticeQuestion(TestTemplate template)
        {
            // get practice questions from template, prioritize questions that were answered the least
            return await con.FindWithQueryAsync<Question>(@"
                SELECT Question.* FROM Template_Question
                INNER JOIN Question ON Template_Question.question = Question.question_id
                LEFT JOIN (
                    SELECT Answer.question, COUNT(*) as cnt FROM PracticeAnswer
                    INNER JOIN Answer ON PracticeAnswer.answer = Answer.answer_id
                    GROUP BY Answer.question
                ) AS AnswerCount ON AnswerCount.question = Question.question_id
                WHERE Template_Question.template = ?
                ORDER BY COALESCE(AnswerCount.cnt, 0)
                LIMIT 1", template.template_id);
        }

        // stats

        public async Task<List<Question>> getAnsweredInTest(Test test, bool answeredCorrect)
        {
            return await con.QueryAsync<Question>(@"
                SELECT Question.*  FROM TestQuestion
                INNER JOIN Question ON Question.question_id = TestQuestion.question
                INNER JOIN Answer ON Answer.question = Question.question_id
                INNER JOIN PracticeAnswer ON PracticeAnswer.answer = Answer.answer_id AND PracticeAnswer.test = TestQuestion.test
                WHERE TestQuestion.test = ? AND Answer.correct = ?
            ", test.test_id, answeredCorrect);
        }
        public async Task<List<Question>> getAnsweredInTest(Test test)
        {
            return await con.QueryAsync<Question>(@"
                SELECT Question.*  FROM TestQuestion
                INNER JOIN Question ON Question.question_id = TestQuestion.question
                INNER JOIN Answer ON Answer.question = Question.question_id
                INNER JOIN PracticeAnswer ON PracticeAnswer.answer = Answer.answer_id AND PracticeAnswer.test = TestQuestion.test
                WHERE TestQuestion.test = ?
            ", test.test_id);
        }

        public async Task<List<Question>> getLastAnsweredInCategory(Category category, bool correct)
        {
            return await con.QueryAsync<Question>(@"
                SELECT Question.*
                FROM TestTemplate
                INNER JOIN Template_Question ON TestTemplate.template_id = Template_Question.template
                INNER JOIN Question ON Template_Question.question = Question.question_id
                INNER JOIN Answer ON Question.question_id = Answer.question
                WHERE TestTemplate.category = ? AND Answer.answer_id IN (SELECT answer_id FROM (
                    SELECT Answer.answer_id, Answer.correct, MAX(PracticeAnswer.answered_time) as _ FROM Answer
                    INNER JOIN PracticeAnswer ON Answer.answer_id = PracticeAnswer.answer
                    GROUP BY Answer.question)
                    WHERE correct = ?
                )
                GROUP BY Question.question_id
            ", category.category_id, correct);
        }

        public async Task<List<Question>> getQuestionsInCategory(Category category)
        {
            return await con.QueryAsync<Question>(@"
                SELECT Question.*
                FROM TestTemplate
                INNER JOIN Template_Question ON TestTemplate.template_id = Template_Question.template
                INNER JOIN Question ON Template_Question.question = Question.question_id
                WHERE TestTemplate.category = ?
                GROUP BY Question.question_id"
                , category.category_id);
        }

        public async Task<List<Question>> getLastAnsweredInTemplate(TestTemplate testTemplate, bool correct)
        {
            return await con.QueryAsync<Question>(@"
                SELECT Question.*
                FROM TestTemplate
                INNER JOIN Template_Question ON TestTemplate.template_id = Template_Question.template
                INNER JOIN Question ON Template_Question.question = Question.question_id
                INNER JOIN Answer ON Question.question_id = Answer.question
                WHERE TestTemplate.template_id = ? AND Answer.answer_id IN (SELECT answer_id FROM (
                    SELECT Answer.answer_id, Answer.correct, MAX(PracticeAnswer.answered_time) as _ FROM Answer
                    INNER JOIN PracticeAnswer ON Answer.answer_id = PracticeAnswer.answer
                    GROUP BY Answer.question)
                    WHERE correct = ?
                )
                GROUP BY Question.question_id
            ", testTemplate.template_id, correct);
        }

        public async Task<int> getQuestionCount()
        {
            return await con.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Question");
        }
    }



}

