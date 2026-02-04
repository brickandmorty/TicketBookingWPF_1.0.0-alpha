using System;

namespace TicketBookingWPF.Model
{
    public class TicketBooking
    {
        public int Id { get; set; }

        public int PhysicalTicketId { get; set; }
        public virtual PhysicalTicket PhysicalTicket { get; set; } = null!;

        public DateTime BookingDate { get; set; }   // always Date (00:00:00)
        public string BookerName { get; set; } = "";

        public double Price { get; set; }
        public bool IsReturnedOrCompleted { get; set; }
        public string? Note { get; set; }
    }
}
