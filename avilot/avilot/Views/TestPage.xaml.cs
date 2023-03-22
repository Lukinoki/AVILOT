using avilot.AVQuestionsEngine;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace avilot.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {

        public static async Task<TestPage> CreateTestPageAsync(QuestionsCollection Collection, int id) {



            var Questions = await Collection.GetAllQuestions();

            var Question = Questions[0];

            var QuestionCount = await Collection.GetQuestionCount();
            var AnsweredQuestionsCount = await Collection.GetAnsweredCount();
            var AnsweredCorrectCount = await Collection.GetAnsweredCorrectCount();
            var AnsweredWrongCount = await Collection.GetAnsweredWrongCount();

            TestPage questionPage = new TestPage(Question, QuestionCount, AnsweredQuestionsCount, AnsweredCorrectCount, AnsweredWrongCount);
            

            return questionPage;
        }
        
        public TestPage(Question Question, int QuestionCount, int AnsweredQuestionsCount, int AnsweredCorrectCount, int AnsweredWrongCount) { 

            InitializeComponent();
            question.Text = Question.Text;
            AllCount.Text = QuestionCount.ToString();
            AnsweredCount.Text = AnsweredQuestionsCount.ToString();
            CorrectAnswers.Text = AnsweredCorrectCount.ToString();
            WrongAnswers.Text = AnsweredWrongCount.ToString();
            

            // generate button for each answer

            for (var i = 0; i < Question.Answers.Count(); i++)
            {
                var answer = new Button { Text = Question.Answers[i].Text, Margin = 5, BackgroundColor = Color.FromHex("#20FFFFFF"), CornerRadius = 15, FontFamily = "Inter", FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)) }; Answers.Children.Add(answer);
    
                //Question.Select(i);


                if (i == Question.CorrectAnswerIndex)
                {
                    answer.Clicked += Correct;
                }
                else
                {
                    answer.Clicked += Wrong;
                }
            }
        }

        private void Wrong(object sender, EventArgs e)
        {
            var b = sender as Button;
         
            b.BackgroundColor = Color.Red;
            b.BorderWidth = 3;
            b.BorderColor = Color.Red;
            //test.Opacity = 0.3;

        }

        private void Correct(object sender, EventArgs e)
        {
            var b = sender as Button;

            b.BackgroundColor = Color.FromHex("#1F00FF19");
            b.BorderWidth = 3;
            b.BorderColor = Color.FromHex("#9900FF19");
            
        }

        private void Like(object sender, EventArgs e) {
            
        }

        private async void Back(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

    }
}