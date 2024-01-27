using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AVILOT.Views
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

        private void NavigateTo(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new MainPage());

        }


    }
}