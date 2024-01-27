using AVILOT.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace AVILOT.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}