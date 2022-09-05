using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading.Tasks;
namespace AVILOT.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new DatasetImportPageV2());
        }

        private async Task daoaod()
        {
            var qs = await AVQuestionsEngine.QuestionsEngine.allQuestionsCollection.GetAllQuestions();
            var q = qs[0];
            await Navigation.PushModalAsync(new TestPage21(q));
        }

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            daoaod();
        }
    }
}