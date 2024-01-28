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
