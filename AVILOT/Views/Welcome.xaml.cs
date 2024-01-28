using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static AVILOT.App;
using Xamarin.Essentials;
using System.IO;

namespace AVILOT.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Welcome : ContentPage
    {
        public Welcome()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private async void NavigateTo(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Region(),false);

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

        }
    }
}