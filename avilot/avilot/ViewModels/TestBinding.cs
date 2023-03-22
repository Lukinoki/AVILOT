using System;
using System.Collections.Generic;
using System.Text;
using avilot.AVQuestionsEngine;

namespace avilot.ViewModels
{
    internal class TestBinding
    {
        public Question Question { get; }
        public TestBinding(Question question)
        {
            Question = question;
        }
    }
}
