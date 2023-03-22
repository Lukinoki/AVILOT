using avilot.Services;
using avilot.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

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


            MainPage = new NavigationPage(new MainPage());


            Device.SetFlags(new string[] { "Brush_Experimental" });

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
