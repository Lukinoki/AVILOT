using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AVILOT.ViewModels;

namespace AVILOT.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DatasetImportPageV2 : ContentPage
    {
        public DatasetImportPageV2()
        {
            InitializeComponent();
            BindingContext = new ViewModels.DatasetImportViewModel();
        }
    }
}