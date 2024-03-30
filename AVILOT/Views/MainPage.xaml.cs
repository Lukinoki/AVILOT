using System;
using System.Linq;
using Xamarin.Forms;
using static AVILOT.App;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;
using System.Runtime.InteropServices.ComTypes;
using AVILOT.Backend.Models;
using AVILOT.Backend;
using Xamarin.Essentials;
using System.Collections.Generic;

namespace AVILOT.Views
{
    public partial class MainPage : ContentPage
    {
        protected async override void OnAppearing() // I changed this to make it "work", TODO: make it work in the intended way
        {
            base.OnAppearing();

            Console.WriteLine("here");
            
            // change airplane type
            var testTemplates = await BackendService.db.getTestTemplates(BackendService.selectedCategory);
            BindableLayout.SetItemsSource(AllCollections, testTemplates);


            SelectedCategoryLabel.BindingContext = BackendService.selectedCategory;

            // last started test 
            var activeTests = await BackendService.db.getActiveTests(BackendService.selectedCategory);
            if (activeTests.Count() > 0)
            {
                var lastTest = activeTests.Last();
                var lastTestTemplate = await BackendService.db.getTestTestTemplate(lastTest);
                continueLastTestButton.BindingContext = lastTestTemplate;
                continueLastTest.Text = lastTestTemplate.headline;
            }
            else
            {
                var randomTestTemplate = testTemplates.ElementAt(new Random().Next(testTemplates.Count()));
                continueLastTestButton.BindingContext = randomTestTemplate;
                continueLastTest.Text = "Start new random";


            }
            // progress
            // TODO: lukas, change the round thing to show percentage of how big percentage the user already answered correct, instead of current test


            var answeredCorrectCount = (await BackendService.db.getLastAnsweredInCategory(BackendService.selectedCategory, true)).Count;
            var answeredWrongCount = (await BackendService.db.getLastAnsweredInCategory(BackendService.selectedCategory, false)).Count;
            var numberOfQuestions = (await BackendService.db.getQuestionsInCategory(BackendService.selectedCategory)).Count;


            Console.WriteLine($"{answeredCorrectCount} {answeredWrongCount} {numberOfQuestions}");
            progress.Text = ((int)((float)answeredCorrectCount / (float)numberOfQuestions * 100)).ToString();
            progressBar.Progress = (float)answeredCorrectCount / (float)numberOfQuestions;
            // streak
            streak.Text = 12.ToString();

            Console.WriteLine("out");
        }

        public MainPage()
        {
            InitializeComponent();
        }


        private async void NavigateToTestPage(object sender, EventArgs e) // also "changed"
        {
            this.IsEnabled = false;
            var testTemplate = (TestTemplate)((Button)sender).BindingContext;
            var activeTests = await BackendService.db.getActiveTests(testTemplate);
            Test test = activeTests.Find(t => t.template.Equals(testTemplate.template_id));
            if (test == null)
            {
                test = await BackendService.db.startTest(testTemplate);
            }
            await Navigation.PushAsync(await TestPage.CreateTestPageAsync(test));
            this.IsEnabled = true;

        }

        private async void NavigateToResultPage(object sender, EventArgs e) // also "changed"
        {
            this.IsEnabled = false;
            var testTemplate = (TestTemplate)((Button)sender).BindingContext;
            var test = await BackendService.db.startTest(testTemplate); // docasne
            await Navigation.PushAsync(await ResultPage.CreateResultPageAsync(test));
            this.IsEnabled = true;
        }

        private void DeleteAnswers(object sender, EventArgs e)
        {
            Debug.WriteLine("TODO: delete wrong answers");
        }

        private async void ChangeType(object sender, EventArgs e)
        {

            Xamarin.Forms.ImageButton buttonImage = (Xamarin.Forms.ImageButton)sender;

            if (ChangeDatabaseMenu.HeightRequest == 0)
            {
                var databases = await BackendService.db.getCategories();

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

        private async void OpenDebugPage(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DatasetImportPageV2());
        }
    }
}
