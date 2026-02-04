using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using TicketBookingWPF.Repository;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace TicketBookingWPF.Services
{
    public class ExportService
    {
        private readonly BookingRepository _repo;

        public ExportService(BookingRepository repo)
        {
            _repo = repo;
            
            // EPPlus Lizenzkontext setzen (für EPPlusFree erforderlich)
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public string ExportToPdf(DateTime startDate, DateTime endDate, string filePath)
        {
            using (var db = new Data.BookingDbContext())
            {
                var bookings = db.Bookings
                    .Include(b => b.PhysicalTicket)
                    .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate)
                    .OrderBy(b => b.BookingDate)
                    .ThenBy(b => b.PhysicalTicket.TicketCode)
                    .ToList();

                try
                {
                    using (var document = new Document(PageSize.A4.Rotate()))
                    {
                        using (var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create)))
                        {
                            document.Open();

                            // Schriftarten definieren
                            var fontTitle = FontFactory.GetFont("Arial", 18, Font.BOLD);
                            var fontHeader = FontFactory.GetFont("Arial", 10, Font.BOLD);
                            var fontNormal = FontFactory.GetFont("Arial", 9, Font.NORMAL);
                            var fontSmall = FontFactory.GetFont("Arial", 8, Font.ITALIC);

                            // Titel
                            var title = new Paragraph($"Buchungsübersicht\n{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}\n\n", fontTitle)
                            {
                                Alignment = Element.ALIGN_CENTER
                            };
                            document.Add(title);

                            // Zusammenfassung
                            var summary = new Paragraph($"Gesamtanzahl Buchungen: {bookings.Count}\n\n", fontNormal);
                            document.Add(summary);

                            // Tabelle erstellen (7 Spalten)
                            var table = new PdfPTable(7)
                            {
                                WidthPercentage = 100
                            };
                            table.SetWidths(new float[] { 12f, 10f, 18f, 10f, 15f, 8f, 27f });

                            // Header-Zeilen
                            string[] headers = { "Datum", "Ticket", "Bucher", "Preis (€)", "Status", "ID", "Notiz" };
                            foreach (var header in headers)
                            {
                                var cell = new PdfPCell(new Phrase(header, fontHeader))
                                {
                                    BackgroundColor = new BaseColor(220, 220, 220),
                                    Padding = 5,
                                    HorizontalAlignment = Element.ALIGN_LEFT
                                };
                                table.AddCell(cell);
                            }

                            // Daten-Zeilen
                            foreach (var booking in bookings)
                            {
                                AddTableCell(table, booking.BookingDate.ToString("dd.MM.yyyy"), fontNormal);
                                AddTableCell(table, booking.PhysicalTicket?.TicketCode ?? "N/A", fontNormal);
                                AddTableCell(table, booking.BookerName, fontNormal);
                                AddTableCell(table, booking.Price.ToString("F2"), fontNormal);
                                AddTableCell(table, booking.IsReturnedOrCompleted ? "Abgeschlossen" : "Offen", fontNormal);
                                AddTableCell(table, booking.Id.ToString(), fontNormal);
                                AddTableCell(table, booking.Note ?? "-", fontNormal);
                            }

                            document.Add(table);

                            // Footer
                            document.Add(new Paragraph($"\n\nErstellt am: {DateTime.Now:dd.MM.yyyy HH:mm} Uhr", fontSmall));

                            document.Close();
                        }
                    }

                    return filePath;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fehler beim PDF-Export: {ex.Message}", ex);
                }
            }
        }

        public string ExportToExcel(DateTime startDate, DateTime endDate, string filePath)
        {
            using (var db = new Data.BookingDbContext())
            {
                var bookings = db.Bookings
                    .Include(b => b.PhysicalTicket)
                    .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate)
                    .OrderBy(b => b.BookingDate)
                    .ThenBy(b => b.PhysicalTicket.TicketCode)
                    .ToList();

                try
                {
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Buchungen");

                        // Titel
                        worksheet.Cells["A1"].Value = $"Buchungsübersicht: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
                        worksheet.Cells["A1:G1"].Merge = true;
                        worksheet.Cells["A1"].Style.Font.Size = 16;
                        worksheet.Cells["A1"].Style.Font.Bold = true;
                        worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // Zusammenfassung
                        worksheet.Cells["A2"].Value = $"Gesamtanzahl: {bookings.Count} Buchungen";
                        worksheet.Cells["A2:G2"].Merge = true;
                        worksheet.Cells["A2"].Style.Font.Italic = true;

                        // Header (Zeile 4)
                        int row = 4;
                        worksheet.Cells[row, 1].Value = "Datum";
                        worksheet.Cells[row, 2].Value = "Ticket-Code";
                        worksheet.Cells[row, 3].Value = "Bucher";
                        worksheet.Cells[row, 4].Value = "Preis (€)";
                        worksheet.Cells[row, 5].Value = "Status";
                        worksheet.Cells[row, 6].Value = "Buchungs-ID";
                        worksheet.Cells[row, 7].Value = "Notiz";

                        // Header formatieren
                        using (var range = worksheet.Cells[row, 1, row, 7])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                        }

                        // Daten
                        row = 5;
                        int startDataRow = row;
                        foreach (var booking in bookings)
                        {
                            worksheet.Cells[row, 1].Value = booking.BookingDate.ToString("dd.MM.yyyy");
                            worksheet.Cells[row, 2].Value = booking.PhysicalTicket?.TicketCode ?? "N/A";
                            worksheet.Cells[row, 3].Value = booking.BookerName;
                            worksheet.Cells[row, 4].Value = booking.Price;
                            worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                            worksheet.Cells[row, 5].Value = booking.IsReturnedOrCompleted ? "Abgeschlossen" : "Offen";
                            worksheet.Cells[row, 6].Value = booking.Id;
                            worksheet.Cells[row, 7].Value = booking.Note ?? "-";

                            row++;
                        }

                        // Spaltenbreite anpassen
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        // Mindestbreiten setzen
                        worksheet.Column(1).Width = 12;  // Datum
                        worksheet.Column(2).Width = 12;  // Ticket-Code
                        worksheet.Column(3).Width = 20;  // Bucher
                        worksheet.Column(4).Width = 12;  // Preis
                        worksheet.Column(5).Width = 15;  // Status
                        worksheet.Column(6).Width = 10;  // ID
                        worksheet.Column(7).Width = 35;  // Notiz

                        // Summe (nur wenn Buchungen vorhanden)
                        if (bookings.Count > 0)
                        {
                            row++;
                            worksheet.Cells[row, 3].Value = "Gesamtsumme:";
                            worksheet.Cells[row, 3].Style.Font.Bold = true;
                            worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            worksheet.Cells[row, 4].Formula = $"SUM(D{startDataRow}:D{row - 1})";
                            worksheet.Cells[row, 4].Style.Font.Bold = true;
                            worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00 \"€\"";
                        }

                        // Footer
                        row += 2;
                        worksheet.Cells[row, 1].Value = $"Erstellt am: {DateTime.Now:dd.MM.yyyy HH:mm} Uhr";
                        worksheet.Cells[row, 1].Style.Font.Italic = true;
                        worksheet.Cells[row, 1].Style.Font.Size = 9;

                        // Datei speichern
                        package.SaveAs(new FileInfo(filePath));
                    }

                    return filePath;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fehler beim Excel-Export: {ex.Message}", ex);
                }
            }
        }

        private void AddTableCell(PdfPTable table, string text, Font font)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                Padding = 5,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            table.AddCell(cell);
        }
    }
}