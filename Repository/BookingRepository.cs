using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using TicketBookingWPF.Data;
using TicketBookingWPF.Model;

namespace TicketBookingWPF.Repository
{
    public class BookingRepository
    {
        public void EnsureDefaultTickets(int count = 10)
        {
            using var db = new BookingDbContext();
            if (db.Tickets.Any()) return;

            for (int i = 1; i <= count; i++)
                db.Tickets.Add(new PhysicalTicket { TicketCode = $"KT-{i:000}", IsActive = true });

            db.SaveChanges();
        }

        public List<PhysicalTicket> GetAllTickets()
        {
            using var db = new BookingDbContext();
            return db.Tickets.Where(t => t.IsActive)
                             .OrderBy(t => t.TicketCode)
                             .ToList();
        }

        public List<TicketBooking> GetBookingsForDate(DateTime date)
        {
            date = date.Date;
            using var db = new BookingDbContext();

            return db.Bookings
                     .Include(b => b.PhysicalTicket)
                     .Where(b => b.BookingDate == date)
                     .ToList();
        }

        public List<TicketBooking> GetBookingsInRange(DateTime from, DateTime to)
        {
            from = from.Date;
            to = to.Date;

            using var db = new BookingDbContext();

            // Keine Projektion in der Query - direkt Entities laden
            return db.Bookings
                .Where(b => b.BookingDate >= from && b.BookingDate <= to)
                .ToList();
        }

        public TicketBooking? GetBookingById(int bookingId)
        {
            using var db = new BookingDbContext();
            return db.Bookings.Include(b => b.PhysicalTicket)
                              .FirstOrDefault(b => b.Id == bookingId);
        }

        public void AddBooking(int ticketId, DateTime date, string bookerName, double price, bool completed, string? note)
        {
            date = date.Date;
            using var db = new BookingDbContext();

            bool alreadyBooked = db.Bookings.Any(b => b.PhysicalTicketId == ticketId && b.BookingDate == date);
            if (alreadyBooked)
                throw new InvalidOperationException("Dieses Ticket ist an diesem Tag bereits gebucht.");

            db.Bookings.Add(new TicketBooking
            {
                PhysicalTicketId = ticketId,
                BookingDate = date,
                BookerName = bookerName.Trim(),
                Price = price,
                IsReturnedOrCompleted = completed,
                Note = note
            });

            db.SaveChanges();
        }

        public void DeleteBooking(int bookingId)
        {
            using var db = new BookingDbContext();
            var booking = db.Bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking == null) return;

            db.Bookings.Remove(booking);
            db.SaveChanges();
        }

        // For Copy: find next date not yet booked for this ticket
        public DateTime FindNextAvailableDateForTicket(int ticketId, DateTime startDate, int maxDaysToCheck = 365)
        {
            startDate = startDate.Date;

            using var db = new BookingDbContext();

            var bookedDates = db.Bookings
                .Where(b => b.PhysicalTicketId == ticketId && b.BookingDate >= startDate)
                .Select(b => b.BookingDate)
                .ToList()
                .Select(d => d.Date)
                .ToHashSet();

            for (int i = 0; i <= maxDaysToCheck; i++)
            {
                var candidate = startDate.AddDays(i);
                if (!bookedDates.Contains(candidate))
                    return candidate;
            }

            throw new InvalidOperationException($"Kein freier Tag in den nächsten {maxDaysToCheck} Tagen gefunden.");
        }
    }
}
