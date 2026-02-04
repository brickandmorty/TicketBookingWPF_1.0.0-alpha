using System.Windows;
using TicketBookingWPF.ViewModel;

namespace TicketBookingWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
