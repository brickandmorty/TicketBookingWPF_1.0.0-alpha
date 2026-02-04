using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TicketBookingWPF.Model;

namespace TicketBookingWPF.ViewModel
{
    public class NewBookingDialogViewModel : NotifyPropertyChangedBase
    {
        public ObservableCollection<PhysicalTicket> Tickets { get; }

        private PhysicalTicket? _selectedTicket;
        public PhysicalTicket? SelectedTicket
        {
            get => _selectedTicket;
            set { _selectedTicket = value; OnPropertyChanged(); SaveCommand.RaiseCanExecuteChanged(); }
        }

        private DateTime? _bookingDate = DateTime.Today;
        public DateTime? BookingDate
        {
            get => _bookingDate;
            set { _bookingDate = value?.Date; OnPropertyChanged(); SaveCommand.RaiseCanExecuteChanged(); }
        }

        private string _bookerName = "";
        public string BookerName
        {
            get => _bookerName;
            set { _bookerName = value; OnPropertyChanged(); SaveCommand.RaiseCanExecuteChanged(); }
        }

        private double _price;
        public double Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        private bool _isReturnedOrCompleted;
        public bool IsReturnedOrCompleted
        {
            get => _isReturnedOrCompleted;
            set { _isReturnedOrCompleted = value; OnPropertyChanged(); }
        }

        private string? _note;
        public string? Note
        {
            get => _note;
            set { _note = value; OnPropertyChanged(); }
        }

        public RelayCommand SaveCommand { get; }

        public TicketBooking? ResultBooking { get; private set; }

        public Action<bool>? CloseAction { get; set; }

        public NewBookingDialogViewModel(
            IEnumerable<PhysicalTicket> tickets,
            DateTime defaultDate,
            int? defaultTicketId = null,
            string? defaultBookerName = null,
            double? defaultPrice = null,
            bool? defaultCompleted = null,
            string? defaultNote = null)
        {
            Tickets = new ObservableCollection<PhysicalTicket>(tickets);

            // SaveCommand ZUERST initialisieren
            SaveCommand = new RelayCommand(Save, CanSave);

            // DANN die Properties setzen
            BookingDate = defaultDate.Date;

            if (defaultTicketId.HasValue)
                SelectedTicket = Tickets.FirstOrDefault(t => t.Id == defaultTicketId.Value);

            BookerName = defaultBookerName ?? "";
            Price = defaultPrice ?? 0.0;
            IsReturnedOrCompleted = defaultCompleted ?? false;
            Note = defaultNote;
        }

        private bool CanSave()
            => SelectedTicket != null
               && BookingDate.HasValue
               && !string.IsNullOrWhiteSpace(BookerName);

        private void Save()
        {
            ResultBooking = new TicketBooking
            {
                PhysicalTicketId = SelectedTicket!.Id,
                BookingDate = BookingDate!.Value.Date,
                BookerName = BookerName.Trim(),
                Price = Price,
                IsReturnedOrCompleted = IsReturnedOrCompleted,
                Note = Note
            };

            CloseAction?.Invoke(true);
        }
    }
}
