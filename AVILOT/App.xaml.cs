using AVILOT.Services;
using AVILOT.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace AVILOT
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
            public static int StreakCount
            {
                get => Preferences.Get(nameof(StreakCount), 0);
                set => Preferences.Set(nameof(StreakCount), value);
            }
            public static DateTime LastCheckedDate
            {
                get => Preferences.Get(nameof(LastCheckedDate), DateTime.Today);
                set => Preferences.Set(nameof(LastCheckedDate), value);
            }
            public static bool DBInit
            {
                get => Preferences.Get(nameof(DBInit), true);
                set => Preferences.Set(nameof(DBInit), value);
            }
            public static bool DBInitialized
            {
                get => Preferences.Get(nameof(DBInitialized), false);
                set => Preferences.Set(nameof(DBInitialized), value);
            }
            public static string SelectedCategory
            {
                get => Preferences.Get(nameof(SelectedCategory), "");
                set => Preferences.Set(nameof(SelectedCategory), value);
            }
            public static string DatasetVersion
            {
                get => Preferences.Get(nameof(DatasetVersion), "");
                set => Preferences.Set(nameof(DatasetVersion), value);
            }

            public static string AppVersion
            {
                get => Xamarin.Essentials.AppInfo.VersionString;
            }
        }

        public App()
        {
            InitializeComponent();

            Device.SetFlags(new string[] { "Brush_Experimental" });
            


            MainPage = new NavigationPage(new LoadingPage());


        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

    }
}
