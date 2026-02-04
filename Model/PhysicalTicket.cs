using System.Collections.Generic;

namespace TicketBookingWPF.Model
{
    public class PhysicalTicket
    {
        public int Id { get; set; }
        public string TicketCode { get; set; } = "";
        public bool IsActive { get; set; } = true;

        public virtual ICollection<TicketBooking> Bookings { get; set; } = new List<TicketBooking>();
    }
}
