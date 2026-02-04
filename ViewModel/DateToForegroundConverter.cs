using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace TicketBookingWPF.ViewModel
{
    public class DateToForegroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Brushes.Black;

            // Im CalendarDayButton Template ist der DataContext direkt das DateTime-Objekt
            if (values[0] is DateTime date && values[1] is IEnumerable<DateTime> fullyBookedDates)
            {
                return fullyBookedDates.Contains(date.Date)
                    ? Brushes.White // Weiﬂe Schrift auf rotem Hintergrund
                    : Brushes.Black;
            }

            return Brushes.Black;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}