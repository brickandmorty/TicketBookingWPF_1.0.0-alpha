using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using TicketBookingWPF.Repository;
using TicketBookingWPF.Services;
using TicketBookingWPF.View;

namespace TicketBookingWPF.ViewModel
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        private readonly BookingRepository _repo = new BookingRepository();
        private readonly ExportService _exportService;

        public ObservableCollection<TicketStatusItem> Tickets { get; } = new ObservableCollection<TicketStatusItem>();

        private TicketStatusItem? _selectedTicket;
        public TicketStatusItem? SelectedTicket
        {
            get => _selectedTicket;
            set { _selectedTicket = value; OnPropertyChanged(); UpdateCommands(); }
        }

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value.Date; OnPropertyChanged(); RefreshTicketStatuses(); }
        }

        private string _statusText = "Bereit";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        // Commands für Verwalten
        public RelayCommand NewBookingCommand { get; }
        public RelayCommand CopyBookingCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ExitCommand { get; }

        // Neue Commands
        public RelayCommand ShowBookingOverviewCommand { get; }
        public RelayCommand ManageTicketsCommand { get; }
        public RelayCommand ShowSettingsCommand { get; }
        public RelayCommand ExportPdfCommand { get; }
        public RelayCommand ExportExcelCommand { get; }

        private HashSet<DateTime> _fullyBookedDates = new HashSet<DateTime>();
        public HashSet<DateTime> FullyBookedDates
        {
            get => _fullyBookedDates;
            private set
            {
                _fullyBookedDates = value;
                OnPropertyChanged();
            }
        }

        private string _fullyBookedDatesText = string.Empty;
        public string FullyBookedDatesText
        {
            get => _fullyBookedDatesText;
            private set
            {
                _fullyBookedDatesText = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            _exportService = new ExportService(_repo);

            // Bestehende Commands
            NewBookingCommand = new RelayCommand(OpenNewDialog);
            CopyBookingCommand = new RelayCommand(OpenCopyDialog, CanCopy);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());

            // Neue Commands
            ShowBookingOverviewCommand = new RelayCommand(ShowBookingOverview);
            ManageTicketsCommand = new RelayCommand(ManageTickets);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            ExportPdfCommand = new RelayCommand(ExportToPdf);
            ExportExcelCommand = new RelayCommand(ExportToExcel);

            _repo.EnsureDefaultTickets(10);
            LoadTicketsOnce();
            RefreshTicketStatuses();
        }

        private void LoadTicketsOnce()
        {
            Tickets.Clear();
            foreach (var t in _repo.GetAllTickets())
                Tickets.Add(new TicketStatusItem(t.Id, t.TicketCode));
        }

        private void RefreshTicketStatuses()
        {
            // 1) Status for selected day
            var bookingsToday = _repo.GetBookingsForDate(SelectedDate);
            var todayMap = bookingsToday.ToDictionary(b => b.PhysicalTicketId, b => b);

            // 2) Range for tooltip next free date (60 days)
            var rangeStart = SelectedDate.Date;
            var rangeEnd = SelectedDate.Date.AddDays(60);
            var bookingsRange = _repo.GetBookingsInRange(rangeStart, rangeEnd);

            var bookedByTicket = bookingsRange
                .GroupBy(b => b.PhysicalTicketId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.BookingDate.Date).ToHashSet());

            int bookedCount = 0;

            foreach (var item in Tickets)
            {
                if (todayMap.TryGetValue(item.TicketId, out var b))
                {
                    item.IsBooked = true;
                    item.BookerName = b.BookerName;
                    item.BookingId = b.Id;
                    bookedCount++;
                }
                else
                {
                    item.IsBooked = false;
                    item.BookerName = null;
                    item.BookingId = null;
                }

                // Next free date in the range
                var set = bookedByTicket.TryGetValue(item.TicketId, out var hs) ? hs : new System.Collections.Generic.HashSet<DateTime>();

                DateTime? nextFree = null;
                for (int i = 0; i <= 60; i++)
                {
                    var candidate = rangeStart.AddDays(i);
                    if (!set.Contains(candidate))
                    {
                        nextFree = candidate;
                        break;
                    }
                }
                item.NextFreeDate = nextFree;
            }

            // Berechne vollausgebuchte Tage
            CalculateFullyBookedDates(rangeStart, rangeEnd, bookedByTicket);

            // Aktualisiere die Liste der ausgebuchten Tage für den aktuellen Monat
            UpdateFullyBookedDatesForCurrentMonth();

            int freeCount = Tickets.Count - bookedCount;
            StatusText = $"{freeCount} frei / {bookedCount} belegt am {SelectedDate:dd.MM.yyyy}";

            UpdateCommands();
        }

        private void CalculateFullyBookedDates(DateTime from, DateTime to, Dictionary<int, HashSet<DateTime>> bookedByTicket)
        {
            var fullyBooked = new HashSet<DateTime>();
            int totalTickets = Tickets.Count;

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                int bookedOnThisDay = 0;
                foreach (var ticketId in Tickets.Select(t => t.TicketId))
                {
                    if (bookedByTicket.TryGetValue(ticketId, out var dates) && dates.Contains(date))
                    {
                        bookedOnThisDay++;
                    }
                }

                if (bookedOnThisDay == totalTickets)
                {
                    fullyBooked.Add(date);
                }
            }

            // Immer eine neue Instanz setzen, um PropertyChanged zu triggern
            // Wichtig, weil sonst bei FullyBookedDates nur der Inhalt geändert wird, aber nicht die Referenz
            // und WPF somit kein Update der Bindings durchführt.

            FullyBookedDates = new HashSet<DateTime>(fullyBooked);
        }


        // Neue Methode für Lösung des FullyBookedBAckground Proplems, die die Liste der vollständig ausgebuchten Tage für den aktuellen Monat aktualisiert
        // dadurch hat der User immer die Information, welche Tage im aktuellen Monat bereits komplett ausgebucht sind, ohne dass er den Kalender die konrketen Tage anklicken muss
        private void UpdateFullyBookedDatesForCurrentMonth()
        {
            // Hole das Jahr und den Monat des ausgewählten Datums
            var selectedYear = SelectedDate.Year;
            var selectedMonth = SelectedDate.Month;

            // Filtere alle vollständig ausgebuchten Tage, die im selben Monat liegen
            var fullyBookedInMonth = FullyBookedDates
                .Where(d => d.Year == selectedYear && d.Month == selectedMonth)
                .OrderBy(d => d.Day)
                .ToList();

            // Erstelle den Anzeigetext
            if (fullyBookedInMonth.Count == 0)
            {
                FullyBookedDatesText = "Keine komplett ausgebuchten Tage in diesem Monat.";
            }
            else
            {
                var monthName = new DateTime(selectedYear, selectedMonth, 1).ToString("MMMM yyyy");
                var datesList = string.Join(", ", fullyBookedInMonth.Select(d => d.ToString("dd.MM.yyyy")));
                FullyBookedDatesText = $"Komplett ausgebuchte Tage im {monthName}:\n{datesList}";
            }
        }

        private void OpenNewDialog(int? defaultTicketId = null,
                                   DateTime? defaultDate = null,
                                   string? defaultBookerName = null,
                                   double? defaultPrice = null,
                                   bool? defaultCompleted = null,
                                   string? defaultNote = null)
        {
            var tickets = _repo.GetAllTickets();

            var vm = new NewBookingDialogViewModel(
                tickets,
                defaultDate ?? SelectedDate,
                defaultTicketId,
                defaultBookerName,
                defaultPrice,
                defaultCompleted,
                defaultNote);

            var dlg = new NewBookingDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            vm.CloseAction = ok => dlg.DialogResult = ok;

            bool? result = dlg.ShowDialog();
            if (result == true && vm.ResultBooking != null)
            {
                try
                {
                    _repo.AddBooking(
                        vm.ResultBooking.PhysicalTicketId,
                        vm.ResultBooking.BookingDate,
                        vm.ResultBooking.BookerName,
                        vm.ResultBooking.Price,
                        vm.ResultBooking.IsReturnedOrCompleted,
                        vm.ResultBooking.Note);

                    StatusText = "Buchung gespeichert.";
                    SelectedDate = vm.ResultBooking.BookingDate;
                    RefreshTicketStatuses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Buchung nicht möglich", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StatusText = ex.Message;
                }
            }
        }

        private void OpenNewDialog()
            => OpenNewDialog(defaultTicketId: SelectedTicket?.TicketId);

        private bool CanDelete()
            => SelectedTicket?.IsBooked == true && SelectedTicket.BookingId.HasValue;

        private void Delete()
        {
            if (SelectedTicket?.BookingId == null) return;

            _repo.DeleteBooking(SelectedTicket.BookingId.Value);
            StatusText = "Buchung gelöscht.";
            RefreshTicketStatuses();
        }

        private bool CanCopy()
            => SelectedTicket?.IsBooked == true && SelectedTicket.BookingId.HasValue;

        private void OpenCopyDialog()
        {
            if (SelectedTicket?.BookingId == null) return;

            var existing = _repo.GetBookingById(SelectedTicket.BookingId.Value);
            if (existing == null)
            {
                StatusText = "Buchung konnte nicht geladen werden.";
                return;
            }

            // Suggest next available date starting from tomorrow (avoid conflict on same day)
            var start = SelectedDate.Date.AddDays(1);

            DateTime suggestedDate;
            try
            {
                suggestedDate = _repo.FindNextAvailableDateForTicket(existing.PhysicalTicketId, start);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Copy nicht möglich", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusText = ex.Message;
                return;
            }

            OpenNewDialog(
                defaultTicketId: existing.PhysicalTicketId,
                defaultDate: suggestedDate,
                defaultBookerName: existing.BookerName,
                defaultPrice: existing.Price,
                defaultCompleted: existing.IsReturnedOrCompleted,
                defaultNote: existing.Note);

            StatusText = $"Vorschlag: nächster freier Tag ist {suggestedDate:dd.MM.yyyy}.";
        }

        private void ShowBookingOverview()
        {
            MessageBox.Show("Buchungsübersicht - wird in einer zukünftigen Version implementiert", "Info", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ManageTickets()
        {
            MessageBox.Show("Ticketverwaltung - wird in einer zukünftigen Version implementiert", "Info", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowSettings()
        {
            MessageBox.Show("Einstellungen - wird in einer zukünftigen Version implementiert", "Info", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportToPdf()
        {
            var dialog = new ExportDialog
            {
                Owner = Application.Current.MainWindow,
                Title = "PDF-Export"
            };

            if (dialog.ShowDialog() == true)
            {
                // Prüfe, ob Datum-Werte gesetzt sind
                if (!dialog.StartDate.HasValue || !dialog.EndDate.HasValue)
                {
                    MessageBox.Show("Bitte wählen Sie einen gültigen Zeitraum aus.", "Fehler",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF-Datei (*.pdf)|*.pdf",
                    FileName = $"Buchungen_{dialog.StartDate.Value:yyyy-MM-dd}_bis_{dialog.EndDate.Value:yyyy-MM-dd}.pdf",
                    DefaultExt = "pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    try
                    {
                        var filePath = _exportService.ExportToPdf(dialog.StartDate.Value.Date, dialog.EndDate.Value.Date, saveDialog.FileName);
                        
                        var result = MessageBox.Show(
                            $"PDF-Export erfolgreich erstellt!\n\nDatei: {filePath}\n\nMöchten Sie die Datei jetzt öffnen?",
                            "Export erfolgreich",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = filePath,
                                UseShellExecute = true
                            });
                        }

                        StatusText = $"PDF exportiert: {Path.GetFileName(filePath)}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fehler beim PDF-Export:\n\n{ex.Message}", "Fehler", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        StatusText = "PDF-Export fehlgeschlagen";
                    }
                }
            }
        }

        private void ExportToExcel()
        {
            var dialog = new ExportDialog
            {
                Owner = Application.Current.MainWindow,
                Title = "Excel-Export"
            };

            if (dialog.ShowDialog() == true)
            {
                // Prüfe, ob Datum-Werte gesetzt sind
                if (!dialog.StartDate.HasValue || !dialog.EndDate.HasValue)
                {
                    MessageBox.Show("Bitte wählen Sie einen gültigen Zeitraum aus.", "Fehler",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel-Datei (*.xlsx)|*.xlsx",
                    FileName = $"Buchungen_{dialog.StartDate.Value:yyyy-MM-dd}_bis_{dialog.EndDate.Value:yyyy-MM-dd}.xlsx",
                    DefaultExt = "xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    try
                    {
                        var filePath = _exportService.ExportToExcel(dialog.StartDate.Value.Date, dialog.EndDate.Value.Date, saveDialog.FileName);
                        
                        var result = MessageBox.Show(
                            $"Excel-Export erfolgreich erstellt!\n\nDatei: {filePath}\n\nMöchten Sie die Datei jetzt öffnen?",
                            "Export erfolgreich",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = filePath,
                                UseShellExecute = true
                            });
                        }

                        StatusText = $"Excel exportiert: {Path.GetFileName(filePath)}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fehler beim Excel-Export:\n\n{ex.Message}", "Fehler", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        StatusText = "Excel-Export fehlgeschlagen";
                    }
                }
            }
        }

        private void UpdateCommands()
        {
            CopyBookingCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
    }
}
