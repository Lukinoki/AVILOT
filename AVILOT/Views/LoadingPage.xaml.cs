using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static AVILOT.App;

namespace AVILOT.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPage : ContentPage
    {
        private bool _loading;
        public LoadingPage()
        {
            InitializeComponent();
            _loading = false;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // if already loading, return
            if (_loading)
            {
                return;
            }
            _loading = true;


            Console.WriteLine("Init Backend running");
            await BackendService.InitializeAsync(Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "db.sqlite"));
            Console.WriteLine("Backend init");


            // on application first run 
            if (Settings.FirstRun)
            {
                Settings.FirstRun = false;
                App.Current.MainPage = new NavigationPage(new Welcome());
            }
            else
            {
                App.Current.MainPage = new NavigationPage(new MainPage());
            }
        }
    }
}