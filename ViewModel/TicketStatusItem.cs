using System;

#nullable enable

namespace TicketBookingWPF.ViewModel
{
    public class TicketStatusItem : NotifyPropertyChangedBase
    {
        public int TicketId { get; }

        private string _ticketCode;
        public string TicketCode
        {
            get => _ticketCode;
            private set => _ticketCode = value;
        }

        private bool _isBooked;
        public bool IsBooked
        {
            get => _isBooked;
            set
            {
                _isBooked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(TooltipText));
            }
        }

        private string? _bookerName;
        public string? BookerName
        {
            get => _bookerName;
            set
            {
                _bookerName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(TooltipText));
            }
        }

        private int? _bookingId;
        public int? BookingId
        {
            get => _bookingId;
            set { _bookingId = value; OnPropertyChanged(); }
        }

        private DateTime? _nextFreeDate;
        public DateTime? NextFreeDate
        {
            get => _nextFreeDate;
            set { _nextFreeDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(TooltipText)); }
        }

        public string StatusText => IsBooked
            ? $"Belegt{(string.IsNullOrWhiteSpace(BookerName) ? "" : $" (von {BookerName})")}"
            : "Frei";

        public string TooltipText
        {
            get
            {
                var datePart = NextFreeDate.HasValue ? NextFreeDate.Value.ToString("dd.MM.yyyy") : "unbekannt";
                return IsBooked
                    ? $"Belegt{(string.IsNullOrWhiteSpace(BookerName) ? "" : $" (von {BookerName})")}. Nächster freier Tag: {datePart}"
                    : $"Frei. Nächster freier Tag: {datePart}";
            }
        }

        public TicketStatusItem(int ticketId, string ticketCode)
        {
            TicketId = ticketId;
            _ticketCode = ticketCode;
        }
    }
}
