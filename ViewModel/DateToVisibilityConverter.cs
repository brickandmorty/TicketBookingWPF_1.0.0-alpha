using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace TicketBookingWPF.ViewModel
{
    public class DateToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Visibility.Collapsed;

            var dataContext = values[0];
            var fullyBookedDates = values[1] as IEnumerable<DateTime>;

            if (dataContext is CalendarDayButton dayButton && fullyBookedDates != null)
            {
                var date = (DateTime)dayButton.DataContext;
                return fullyBookedDates.Contains(date) ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}