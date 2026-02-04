using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace TicketBookingWPF.ViewModel
{
    public class DateToBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Brushes.Transparent;

            // Im CalendarDayButton Template ist der DataContext direkt das DateTime-Objekt
            if (values[0] is DateTime date && values[1] is IEnumerable<DateTime> fullyBookedDates)
            {
                return fullyBookedDates.Contains(date.Date)
                    ? new SolidColorBrush(Color.FromRgb(220, 53, 69)) // Bootstrap Red
                    : Brushes.Transparent;
            }

            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}