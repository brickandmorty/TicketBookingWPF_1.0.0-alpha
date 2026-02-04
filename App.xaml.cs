using System;
using System.Windows;

namespace TicketBookingWPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Set DataDirectory for Entity Framework connection string resolution
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);

            base.OnStartup(e);
        }
    }
}
