using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace avilot.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectCollection : ContentPage
    {
        public SelectCollection()
        {
            InitializeComponent();
        }


        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private async void NavigateTo(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }


    }
}