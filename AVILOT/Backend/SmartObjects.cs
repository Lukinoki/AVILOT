using AVILOT.Backend.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AVILOT.Backend
{
    public abstract class SmartObject
    {
        protected readonly Database db;
        public SmartObject(Database db)
        {
            this.db = db;
        }
    }
    public class SmartCategory : SmartObject
    {
        public readonly Category data;
        public SmartCategory(Category data, Database db) : base(db)
        {
            this.data = data;
        }
        public async Task<List<SmartTestTemplate>> getTestTemplates()
        {
            var templates = await db.getTestTemplates(data);
            return templates.ConvertAll(t => new SmartTestTemplate(t, db));
        }

    }

    public class SmartTestTemplate : SmartObject
    {
        public TestTemplate data;

        public SmartTestTemplate(TestTemplate data, Database db) : base(db)
        {
            this.data = data;
        }

        public async Task<SmartTest> startTest()
        {
            if (!data.testable)
            {
                throw new Exception("Test template is not testable");
            }
            var test = await this.db.startTest(data);
            return new SmartTest(test, db);
        }
    }

    public class SmartTest : SmartObject
    {
        public Test data;
        public SmartTest(Test data, Database db) : base(db)
        {
            this.data = data;
        }
        public async Task<List<SmartQuestion>> getQuestions()
        {
            var questions = await db.getTestQuestions(data);
            return questions.ConvertAll(q => new SmartQuestion(q, data, db));
        }
    }

    public class SmartQuestion : SmartObject
    {
        public Question data;
        public Test? test;
        public SmartQuestion(Question data, Test? test, Database db) : base(db)
        {
            this.data = data;
            this.test = test;
        }
        public async Task<List<SmartAnswer>> getAnswers()
        {
            var answers = await db.getAnswers(data);
            return answers.ConvertAll(a => new SmartAnswer(a, test, db));
        }
    }

    public class SmartAnswer : SmartObject
    {
        public Answer data;
        public Test? test;
        public SmartAnswer(Answer data, Test? test, Database db) : base(db)
        {
            this.data = data;
            this.test = test;
        }
        public async Task submitAnswer()
        {
            if (this.test == null)
            {
                await db.answerPracticeQuestion(data);
            }
            else
            {
                await db.answerTestQuestion(data, test);
            }
        }
    }

    
}
