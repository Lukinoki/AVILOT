using AVILOT.Backend.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.PlatformConfiguration.iOSSpecific;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AVILOT.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {
        private Question currentQuestion;
        private Test currentTest;

        public bool c;

        public static async Task<TestPage> CreateTestPageAsync(Test test)
        {
            var Questions = await BackendService.db.getTestQuestions(test);
            foreach (var q in Questions)
            {
                Console.WriteLine(q);
            }

            Random random = new Random();
            int QuestionId = random.Next(0,Questions.Count);

            var question = Questions[QuestionId];
            var QuestionCount = Questions.Count;
            var AnsweredQuestionsCount = 10; // not implemented yet
            var AnsweredCorrectCount = 7; // not implemented yet
            var AnsweredWrongCount = 3; // not implemented yet

            TestPage questionPage = new TestPage(question, QuestionCount, AnsweredQuestionsCount, AnsweredCorrectCount, AnsweredWrongCount, test);

            await questionPage.InitAsync();

            return questionPage;
        }

        public TestPage(Question Question, int QuestionCount, int AnsweredQuestionsCount, int AnsweredCorrectCount, int AnsweredWrongCount, Test test)
        {

            InitializeComponent();
            question.Text = Question.headline;
            AllCount.Text = QuestionCount.ToString();
            AnsweredCount.Text = AnsweredQuestionsCount.ToString();
            CorrectAnswers.Text = AnsweredCorrectCount.ToString();
            WrongAnswers.Text = AnsweredWrongCount.ToString();
            currentQuestion = Question;
            currentTest = test;
            c = false;

            
        }

        public async Task InitAsync()
        {
            // create answer buttons
            var answers = await BackendService.db.getAnswers(currentQuestion);
            for (var i = 0; i < answers.Count; i++)
            {
                var answerButton = new Button
                {
                    Text = answers[i].answer_text,
                    Margin = 5,
                    BackgroundColor = Color.FromHex("#1FFFFFFF"),
                    CornerRadius = 15,
                    FontFamily = "Inter",
                    FontSize = 22,
                    BindingContext = ((answers[i], currentQuestion)),
                    TextTransform = TextTransform.None,
                    Padding = 10,
                };
                answerButton.Clicked += Answer;

                Answers.Children.Add(answerButton);


            }
        }

        private async void Answer(object sender, EventArgs e) {
            Button answerButton = (Button)sender;
            var (answer, question) = ((Answer, Question))answerButton.BindingContext;

            foreach (var child in ((answerButton).Parent as StackLayout).Children)
            {
                if (child is Button)
                {
                    Button button = (child as Button);
                    button.IsEnabled = false;

                }
            }

            await BackendService.db.answerPracticeQuestion(answer);

            var answers = await BackendService.db.getAnswers(question);
            var correct = answers.Find(a => a.correct);
            
            if (answer.correct)
            {
                answerButton.BackgroundColor = Color.FromHex("#1F00FF19");
                answerButton.BorderWidth = 2;
                answerButton.BorderColor = Color.FromHex("#9900FF19");

                await Task.Delay(1000);

            }
            else
            {
                answerButton.BackgroundColor = Color.FromHex("#1FD10000");
                answerButton.BorderWidth = 2;
                answerButton.BorderColor = Color.FromHex("#99D10000");
                var testp = (TestPage)Navigation.NavigationStack.Last();
                var popup = new WrongAnswerPopup(correct.answer_text) { IsLightDismissEnabled = false };
                await Navigation.ShowPopupAsync(popup);
            }
            await Navigation.PushAsync(await CreateTestPageAsync(currentTest));
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