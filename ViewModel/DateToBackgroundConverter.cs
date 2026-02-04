using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TicketBookingWPF.ViewModel
{
    public class DateToBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Beispiel-Implementierung: Prüfen, ob das Datum ausgebucht ist
            var dataContext = values[0];
            var fullyBookedDates = values[1] as System.Collections.IEnumerable;
            var calendarDay = dataContext as DateTime?;
            if (calendarDay != null && fullyBookedDates != null)
            {
                foreach (var date in fullyBookedDates)
                {
                    if (date is DateTime dt && dt.Date == calendarDay.Value.Date)
                        return Brushes.LightGray; // Ausgebucht
                }
            }
            return Brushes.White; // Nicht ausgebucht
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}