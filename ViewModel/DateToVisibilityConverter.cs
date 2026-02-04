using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;

namespace TicketBookingWPF.ViewModel
{
    public class DateToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Visibility.Collapsed;

            // Im CalendarDayButton Template ist der DataContext direkt das DateTime-Objekt
            if (values[0] is DateTime date && values[1] is IEnumerable<DateTime> fullyBookedDates)
            {
                return fullyBookedDates.Contains(date.Date) ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}