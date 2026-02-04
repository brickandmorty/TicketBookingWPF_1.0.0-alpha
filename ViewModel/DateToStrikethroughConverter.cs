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
                    ? TextDecorations.Strikethrough // hier legen wir das durchstreichen fest, wenn das Datum in der Liste der ausgebuchten Termine ist
                    : null;
            }

            return null;
        }

        // nicht implemetiert, da nur OneWay Binding verwendet wird
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    ///*
    ///Wichtig, für das rote X über ausgebuchten Tagen brauchen wir eine komplett neue Methode; Da TEXTDECORATION nur Strikethrough, Underline etc. zurückgeben kann, 
    ///aber kein Bild, müssen wir das über XAML lösen, indem wir die Sichtbarkeit eines roten X über dem Datum steuern. 
    ///Dafür brauchen wir eine neue Methode, die überprüft, ob das Datum in der Liste der ausgebuchten Termine ist, und dann die Sichtbarkeit des roten X entsprechend anpasst.
    ///Hinweis: Diese Methode könnte ähnlich wie die Convert-Methode in DateToStrikethroughConverter aussehen, aber anstatt TextDecorations zurückzugeben, würde sie Visibility.Visible oder Visibility.Collapsed zurückgeben, je nachdem, ob das Datum ausgebucht ist oder nicht.
    ///
    /// Im Grunde ändern wir bei der Alternative mit dem roten X nur die Sichtbarkeit eines Elements in der XAML, während wir bei der Strikethrough-Methode die Textdekoration des Datums ändern. Beide Methoden überprüfen jedoch, ob das Datum in der Liste der ausgebuchten Termine enthalten ist, um die entsprechende Formatierung oder Sichtbarkeit festzulegen.
    /// Beim roten X müssen wir jedenfalls direkt über die ControlTemplate im XAML zugreifen. Dort können wir dann die Sichtbarkeit eines roten X steuern, das über dem Datum liegt, indem wir die Convert-Methode verwenden, um zu überprüfen, ob das Datum ausgebucht ist, und entsprechend die Sichtbarkeit des roten X anpassen.
    ///
}
