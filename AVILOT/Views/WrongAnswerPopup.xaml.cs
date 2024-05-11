using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AVILOT.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WrongAnswerPopup : Popup
    {
        public WrongAnswerPopup(string correctAnswer)
        {
            InitializeComponent();
            correct.Text = correctAnswer;
        }
        public async void ClosePopup(object sender, EventArgs e)
        {
            Dismiss(null);
        }
    }
}