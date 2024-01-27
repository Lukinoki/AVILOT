using AVILOT.AVQuestionsEngine;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AVILOT.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {

        public static async Task<TestPage> CreateTestPageAsync(QuestionsCollection Collection)
        {

            var Questions = await Collection.GetAllQuestions();
            Random random = new Random();
            int QuestionId = random.Next(0,Questions.Length);

            var question = Questions[QuestionId];
            var QuestionCount = await Collection.GetQuestionCount();
            var AnsweredQuestionsCount = await Collection.GetAnsweredCount();
            var AnsweredCorrectCount = await Collection.GetAnsweredCorrectCount();
            var AnsweredWrongCount = await Collection.GetAnsweredWrongCount();

            TestPage questionPage = new TestPage(question, QuestionCount, AnsweredQuestionsCount, AnsweredCorrectCount, AnsweredWrongCount);

            return questionPage;
        }

        public TestPage(Question Question, int QuestionCount, int AnsweredQuestionsCount, int AnsweredCorrectCount, int AnsweredWrongCount)
        {

            InitializeComponent();
            question.Text = Question.Text;
            AllCount.Text = QuestionCount.ToString();
            AnsweredCount.Text = AnsweredQuestionsCount.ToString();
            CorrectAnswers.Text = AnsweredCorrectCount.ToString();
            WrongAnswers.Text = AnsweredWrongCount.ToString();


            // generate button for each answer

            for (var i = 0; i < Question.Answers.Count(); i++)
            {
                int[] a = new int[] { i, Question.Id };
                var answer = new Button
                {
                    Text = Question.Answers[i].Text,
                    Margin = 5,
                    BackgroundColor = Color.FromHex("#1FFFFFFF"),
                    CornerRadius = 15,
                    FontFamily = "Inter",
                    FontSize = 22,
                    BindingContext = (i, Question),
                    TextTransform = TextTransform.None,
                    Padding=10,
                };
                answer.Clicked += Answer;

                Answers.Children.Add(answer);


            }
        }

        private async void Answer(object sender, EventArgs e) {
            Button answer = (Button)sender;
            (int i, Question Question) = ((int, Question))answer.BindingContext;

            if (i == Question.CorrectAnswerIndex)
            {
                await Question.Select(i);
                answer.BackgroundColor = Color.FromHex("#1F00FF19");
                answer.BorderWidth = 2;
                answer.BorderColor = Color.FromHex("#9900FF19");

            }
            else
            {
                await Question.Select(i);
                answer.BackgroundColor = Color.Red;
            }


            foreach (var child in ((answer).Parent as StackLayout).Children) { 
                if (child is Button)
                {
                    Button button = (child as Button);
                    button.IsEnabled = false;

                }
            }

            await Task.Delay(2000);
            await Navigation.PushAsync(await CreateTestPageAsync(Question.ParentCollection));

        }

        private void Like(object sender, EventArgs e)
        {
            Debug.Write("TODO: like");
        }

         async void Back() 
        {
            await Navigation.PopToRootAsync();
        }
        // on page back button
        private void GoBack(object sender, EventArgs e)
        {
            Back();
        }
        // system back button
        protected override bool OnBackButtonPressed()
        {
            Back();
            return true;
        }

    }
}