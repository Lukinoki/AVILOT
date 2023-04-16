using avilot.Services;
using avilot.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using avilot.AVQuestionsEngine;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace avilot
{
    public partial class App : Application
    {
        public static class Settings
        {
            public static bool FirstRun
            {
                get => Preferences.Get(nameof(FirstRun), true);
                set => Preferences.Set(nameof(FirstRun), value);
            }
        }

        public App()
        {
            InitializeComponent();

            Device.SetFlags(new string[] { "Brush_Experimental" });

            // on application first run 
            if (Settings.FirstRun)
            {
                Task.Run(async () =>
                {
                    // Import question collections to database from csv

                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = "avilot.PLA.csv";
                    using var reader = new StreamReader(assembly.GetManifestResourceStream(resourceName));
                    await QuestionsEngine.ImportQuestionsFromCsv(reader, "Test1");

                    //just test collection with only few questions
                    var assembly2 = Assembly.GetExecutingAssembly();
                    var resourceName2 = "avilot.PLA2.csv";
                    using var reader2 = new StreamReader(assembly2.GetManifestResourceStream(resourceName2));
                    await QuestionsEngine.ImportQuestionsFromCsv(reader2, "Test2");

                    Settings.FirstRun = false;

                }).Wait();

                MainPage = new NavigationPage(new Welcome());

            }
            else
            {
                MainPage = new NavigationPage(new MainPage());
            }


        }
        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
