using AVILOT.Backend.Models;
using AVILOT.Views;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AVILOT.ViewModels.Converters
{
    public class GetQuestionColor : IValueConverter
    {

        public object Convert(object value, Type targetType, object paramater, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                Question question = (Question)value;

                DateTime? LastCorrectAnswerDate = null;
                DateTime? LastWrongAnswerDate = null;

                // changed this to hacky way to make it work
                // TODO: Make it work
                var rng = new Random();
                if (rng.NextDouble() < 0.5)
                {
                    LastCorrectAnswerDate = DateTime.Now;
                }
                if (rng.NextDouble() < 0.5)
                {
                    LastWrongAnswerDate = DateTime.Now;
                }

                if (LastCorrectAnswerDate != null && LastWrongAnswerDate == null)
                {
                    return Color.FromHex("2BFF4D");
                }
                else if (LastCorrectAnswerDate == null && LastWrongAnswerDate != null)
                {
                    return Color.FromHex("FF2929");
                }
                else if (LastCorrectAnswerDate != null && LastWrongAnswerDate != null)
                {
                    return LastCorrectAnswerDate > LastWrongAnswerDate ? Color.FromHex("2BFF4D") : Color.FromHex("FF2929");
                }
                else {
                    return Color.White;
                }
            }

            return Color.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}