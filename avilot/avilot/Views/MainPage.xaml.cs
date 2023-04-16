using System;
using System.Linq;
using Xamarin.Forms;
using static avilot.App;
using System.IO;
using System.Reflection;
using avilot.AVQuestionsEngine;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;
using avilot.AVQuestionsEngine.Database;

namespace avilot.Views
{
    public partial class MainPage : ContentPage
    {
        protected async override void OnAppearing()
        {
            base.OnAppearing();

            // calculate percentage of correct answers
            //await QuestionsCollection.GetAnsweredCorrectPercentage();


            var collections = await QuestionsEngine.GetAllColections();
            for (int i = 0; i < collections.Count(); i++)
            {
                BindableLayout.SetItemsSource(AllCollections, collections);
            }

            // last started test 
            var LastTestedCollection = await QuestionsEngine.GetCollectionById(1);
            continueLastTest.Text = LastTestedCollection.Name;
            continueLastTestButton.BindingContext = LastTestedCollection.Id;

        }

        public MainPage()
        {
            InitializeComponent();
        }


        private async void NavigateTo(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            int id = (int)((Button)sender).BindingContext;
            var questionsCollection = await QuestionsEngine.GetCollectionById(id);


            await Navigation.PushAsync(await TestPage.CreateTestPageAsync(questionsCollection));
            //await Navigation.PushAsync(await ResultPage.CreateResultPageAsync(questionsCollection));
            this.IsEnabled = true;

        }

        private async void Result(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            int id = (int)((Button)sender).BindingContext;
            var questionsCollection = await QuestionsEngine.GetCollectionById(id);
            await Navigation.PushAsync(await ResultPage.CreateResultPageAsync(questionsCollection));
            this.IsEnabled = true;
        }

        private async void DeleteAnswers(object sender, EventArgs e) {

        }

        private async void ChangeDatabase(object sender, EventArgs e) {

            Xamarin.Forms.ImageButton buttonImage = (Xamarin.Forms.ImageButton)sender;

            if (ChangeDatabaseMenu.HeightRequest == 0)
            {
                var databases = await QuestionsEngine.GetAllColections();

                for (int i = 0; i < databases.Count(); i++)
                {
                    BindableLayout.SetItemsSource(AllDatabases, databases);
                }
                ChangeDatabaseMenu.HeightRequest = 180;
                buttonImage.Source = "vector3";
            }
            else
            {
                ChangeDatabaseMenu.HeightRequest = 0;
                buttonImage.Source = "vector2";
            }
        }

    }
}
