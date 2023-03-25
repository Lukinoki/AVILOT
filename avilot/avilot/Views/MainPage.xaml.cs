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

namespace avilot.Views
{
    public partial class MainPage : ContentPage
    {
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if (Settings.FirstRun)
            {
                // import question collections
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "avilot.PLA.csv";
                using var reader = new StreamReader(assembly.GetManifestResourceStream(resourceName));
                await QuestionsEngine.ImportQuestionsFromCsv(reader, "Test1");

                /*
                var assembly2 = Assembly.GetExecutingAssembly();
                var resourceName2 = "avilot.PLA2.csv";
                using var reader2 = new StreamReader(assembly2.GetManifestResourceStream(resourceName2));
                await QuestionsEngine.ImportQuestionsFromCsv(reader2, "Test2");

                var assembly3 = Assembly.GetExecutingAssembly();
                var resourceName3 = "avilot.PLA2.csv";
                using var reader3 = new StreamReader(assembly3.GetManifestResourceStream(resourceName3));
                await QuestionsEngine.ImportQuestionsFromCsv(reader3, "Test3");
                */
                /*
                var assembly4 = Assembly.GetExecutingAssembly();
                var resourceName4 = "avilot.PLA2.csv";
                using var reader4 = new StreamReader(assembly4.GetManifestResourceStream(resourceName4));
                var (questionModels, answerModels) = CsvUtils.LoadQuestionsFromCSV(reader); //Load question models from CSV
                await QuestionsEngine.ImportQuestionsToDatabase("Test1", questionModels, answerModels);
                var c1 = await QuestionsEngine.GetCollectionById(1);
                */


                // start initial dialog
                await Navigation.PushAsync(new Welcome());
                Settings.FirstRun = false;

                // calculate percentage of correct answers
                //await QuestionsCollection.GetAnsweredCorrectPercentage();
            }
            else
            {
                var collections = await QuestionsEngine.GetAllColections();
                for (int i = 0; i < collections.Count(); i++)
                {
                    BindableLayout.SetItemsSource(AllCollections, collections);
                }/*
                // last started test
                var LastTestedCollection = await QuestionsEngine.GetCollectionById(0);
                continueLastTest.Text = LastTestedCollection.Name;
                continueLastTestButton.BindingContext = LastTestedCollection.Id;*/
            }
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

            
            await Navigation.PushAsync(await TestPage.CreateTestPageAsync(questionsCollection, id));
            //await Navigation.PushAsync(await ResultPage.CreateTestPageAsync(questionsCollection));
            this.IsEnabled = true;

        }

    }
}
