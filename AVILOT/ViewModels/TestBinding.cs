using System;
using System.Collections.Generic;
using System.Text;
using AVILOT.AVQuestionsEngine;

namespace AVILOT.ViewModels
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
