using AVILOT.Backend.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static System.Net.Mime.MediaTypeNames;

namespace AVILOT.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResultPage : ContentPage
    {
        public static async Task<ResultPage> CreateResultPageAsync(Test test)
        {
            var Questions = await BackendService.db.getTestQuestions(test);
            var template = await BackendService.db.getTestTestTemplate(test);
            var AnsweredCorrectCount = 7; // not implemented yet
            var AnsweredWrongCount = 3; // not implemented yet

            ResultPage questionPage = new ResultPage(template.headline, Questions, AnsweredCorrectCount, AnsweredWrongCount);

            return questionPage;
        }

        public ResultPage(string Name, List<Question> Questions, int AnsweredCorrectCount, int AnsweredWrongCount)
        {
            InitializeComponent();

            TestName.Text = $"{Name}: Result";
            CorrectAnswers.Text = AnsweredCorrectCount.ToString();
            WrongAnswers.Text = AnsweredWrongCount.ToString();

            BindableLayout.SetItemsSource(QuestionList, Questions);
        }

        private async void ShowAnswers(object sender, EventArgs e)
        {

            Xamarin.Forms.ImageButton buttonImage = (Xamarin.Forms.ImageButton)sender;
            Question question = (Question)buttonImage.BindingContext;
            List<Answer> answers = await BackendService.db.getAnswers(question);
            StackLayout ParentStackLayout = (StackLayout)buttonImage.Parent;
            StackLayout ChildernStackLayout = (StackLayout)ParentStackLayout.Children[1];
            

            if (buttonImage.Source.ToString().Contains("vector1"))
            {
                buttonImage.Source = "vector2";


                for (var i = 0; i < answers.Count; i++)
                {
                    var answer = new Frame
                    {
                        BackgroundColor = Color.FromHex("#1FFFFFFF"),
                        CornerRadius = 15,
                        Margin = 5,
                        Padding = 16,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        HorizontalOptions = LayoutOptions.FillAndExpand,

                        Content = new Label
                        {
                            Text = answers[i].answer_text,
                            FontFamily = "Inter",
                            FontSize = 22,
                            TextTransform = TextTransform.None,
                            TextColor = Color.White,
                        }
                    };
                    if (answers[i].correct) {
                        answer.BackgroundColor = Color.FromHex("#3300FF19");
                    }
                    ChildernStackLayout.Children.Add(answer);
                }
            }
            else
            {
                buttonImage.Source = "vector1";

                for (var i = 0; i < answers.Count; i++)
                {
                    ChildernStackLayout.Children.RemoveAt(1);
                }
            }
        }
        private async void Back(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}