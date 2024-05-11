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
        private int streakCount = 0;
        private DateTime lastCheckedDate;
        protected async override void OnAppearing() // I changed this to make it "work", TODO: make it work in the intended way
        {
            base.OnAppearing();

            // Load streak data from settings
            LoadStreakData();

            // Update streak UI
            UpdateStreakUI();

            CheckInButton_Clicked();

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

            if (ChangeCategoryMenu.HeightRequest == 0)
            {
                var databases = await BackendService.db.getCategories();

                for (int i = 0; i < databases.Count(); i++)
                {
                    BindableLayout.SetItemsSource(AllCategories, databases);
                }
                ChangeCategoryMenu.HeightRequest = 180;
                buttonImage.Source = "vector3";
            }
            else
            {
                ChangeCategoryMenu.HeightRequest = 0;
                buttonImage.Source = "vector2";
            }
        }

        private void LoadStreakData()
        {
            // Retrieve streak count and last checked date from settings
            streakCount = App.Settings.StreakCount;
            lastCheckedDate = App.Settings.LastCheckedDate;
        }
        private void SaveStreakData()
        {
            // Save streak count and last checked date to settings
            App.Settings.StreakCount = streakCount;
            App.Settings.LastCheckedDate = lastCheckedDate;
        }
        private void UpdateStreakUI()
        {
            // Update streak count label
            // streak
            streak.Text = streakCount.ToString();
        }

        private void CheckInButton_Clicked()
        {
            // Check if today is the next day after the last checked date
            if (DateTime.Today == lastCheckedDate.AddDays(1))
            {
                // Increase streak count
                streakCount++;
            }
            else
            {
                // Reset streak count
                streakCount = 1;
            }
            // Update last checked date
            lastCheckedDate = DateTime.Today;
            // Save streak data
            SaveStreakData();
            // Update streak UI
            UpdateStreakUI();
         }

        private async void OpenDebugPage(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DatasetImportPageV2());
        }
    }
}
