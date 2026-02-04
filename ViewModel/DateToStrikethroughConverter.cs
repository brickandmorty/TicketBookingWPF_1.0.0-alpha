using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TicketBookingWPF.ViewModel
{
    public class DateToStrikethroughConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[0] == DependencyProperty.UnsetValue || values[1] == null)
                return null;

            if (values[0] is DateTime date && values[1] is HashSet<DateTime> fullyBookedDates)
            {
                return fullyBookedDates.Contains(date.Date) 
                    ? TextDecorations.Strikethrough 
                    : null;
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
