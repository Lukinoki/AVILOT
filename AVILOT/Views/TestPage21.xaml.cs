using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AVILOT.AVQuestionsEngine;


namespace AVILOT.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage21 : ContentPage
    {
        public string Text { get; set; }
        public Answer[] answers { get; set; }
        public Question question { get; set; }
        public TestPage21(Question questions)
        {
            Text = questions.Text;
            answers = questions.Answers;
            question = question;
            InitializeComponent();
            BindingContext = this;
            
            

        }
    }
}