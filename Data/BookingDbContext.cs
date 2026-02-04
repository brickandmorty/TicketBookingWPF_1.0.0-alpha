using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using TicketBookingWPF.Model;

namespace TicketBookingWPF.Data
{
    public class BookingDbContext : DbContext
    {
        public DbSet<PhysicalTicket> Tickets { get; set; }
        public DbSet<TicketBooking> Bookings { get; set; }

        static BookingDbContext()
        {
            // Creates DB file automatically on first use (LocalDB attach)
            Database.SetInitializer(new CreateDatabaseIfNotExists<BookingDbContext>());
        }

        public BookingDbContext() : base("name=BookingDb")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PhysicalTicket>()
                .Property(t => t.TicketCode)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<TicketBooking>()
                .Property(b => b.BookerName)
                .IsRequired()
                .HasMaxLength(100);

            // Unique index: one booking per (Ticket, Date)
            modelBuilder.Entity<TicketBooking>()
                .Property(b => b.PhysicalTicketId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Ticket_Date", 1) { IsUnique = true }));

            modelBuilder.Entity<TicketBooking>()
                .Property(b => b.BookingDate)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Ticket_Date", 2) { IsUnique = true }));

            base.OnModelCreating(modelBuilder);
        }
    }
}
