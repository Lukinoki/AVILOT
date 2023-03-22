using avilot.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace avilot.Views
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