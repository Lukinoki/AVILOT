using avilot.AVQuestionsEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace avilot.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResultPage : ContentPage
    {
        public static async Task<ResultPage> CreateTestPageAsync(QuestionsCollection Collection)
        {
            var Questions = await Collection.GetAllQuestions();

            var QuestionCount = await Collection.GetQuestionCount();
            var AnsweredQuestionsCount = await Collection.GetAnsweredCount();
            var AnsweredCorrectCount = await Collection.GetAnsweredCorrectCount();
            var AnsweredWrongCount = await Collection.GetAnsweredWrongCount();

            ResultPage questionPage = new ResultPage(Collection.Name, Questions);


            return questionPage;
        }

        public ResultPage(string Name, Question[] Questions)
        {
            InitializeComponent();
            TestName.Text = $"{Name}: Result";

            for (var i = 0; i < Questions.Count(); i++)
            {
                var answer = new Button { Text = Questions[i].Text, Margin = 5, BackgroundColor = Color.FromHex("#20FFFFFF"), CornerRadius = 15, FontFamily = "Inter", FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)) }; list.Children.Add(answer);

            }
        }
    }
}