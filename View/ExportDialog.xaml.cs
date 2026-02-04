using System;
using System.Windows;

namespace TicketBookingWPF.View
{
    public partial class ExportDialog : Window
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public ExportDialog()
        {
            InitializeComponent();
            DataContext = this;
            
            // Standardwerte setzen
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(1);
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

