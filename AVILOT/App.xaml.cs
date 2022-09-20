using AVILOT.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading.Tasks;
namespace AVILOT
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            //app init
            AVQuestionsEngine.QuestionsEngine.Initialize();
            var mpage = new AboutPage();
            var navpage = new NavigationPage(mpage);
            MainPage = navpage;
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
