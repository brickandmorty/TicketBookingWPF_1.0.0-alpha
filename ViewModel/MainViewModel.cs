using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TicketBookingWPF.Repository;
using TicketBookingWPF.View;

namespace TicketBookingWPF.ViewModel
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        private readonly BookingRepository _repo = new BookingRepository();

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

        public RelayCommand NewBookingCommand { get; }
        public RelayCommand CopyBookingCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ExitCommand { get; }

        public MainViewModel()
        {
            NewBookingCommand = new RelayCommand(OpenNewDialog);
            CopyBookingCommand = new RelayCommand(OpenCopyDialog, CanCopy);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());

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

            int freeCount = Tickets.Count - bookedCount;
            StatusText = $"{freeCount} frei / {bookedCount} belegt am {SelectedDate:dd.MM.yyyy}";

            UpdateCommands();
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

        private void UpdateCommands()
        {
            CopyBookingCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
    }
}
