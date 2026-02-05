# KlimaticketBooker – WPF Abschlussprojekt (Interaktive Systeme)

## Projektübersicht
Diese WPF-Applikation dient zur Verwaltung von **physischen Klimatickets**, welche tageweise
gebucht werden können. Pro Tag kann jedes Ticket **maximal einmal** vergeben werden.
Die Anwendung wurde im Rahmen der Lehrveranstaltung **Interaktive Systeme** umgesetzt
und orientiert sich am **MVVM-Pattern** sowie an den Vorgaben der Aufgabenstellung.

---

## Fachliches Konzept
- Es existiert eine fixe Anzahl physischer Tickets (z. B. KT-001 bis KT-010)
- Jedes Ticket kann **nur für einen ganzen Tag** gebucht werden
- Pro Ticket und Datum ist **nur eine Buchung erlaubt**
- Der Benutzer sieht jederzeit:
  - welche Tickets frei sind
  - welche Tickets belegt sind
  - von wem sie gebucht wurden

---

## Architektur (MVVM)
- **Model**
  - `PhysicalTicket`
  - `TicketBooking`
- **ViewModel**
  - `MainViewModel`
  - Zuständig für Logik, Commands und Statusanzeige
- **View**
  - `MainWindow.xaml`
  - XAML mit DataBinding

---

## Aufgabe 1 – GUI mit WPF
- Menüleiste:
  - Datei → Neu, Beenden
  - Bearbeiten → Ausschneiden, Kopieren
- Symbolleiste:
  - Neu, Kopieren, Beenden
- Statuszeile:
  - Anzeige freier / belegter Tickets
  - Benutzerfeedback
- 2-spaltiger Aufbau:
  - Links: Ticketliste
  - Rechts: Detail- & Eingabebereich

---

## Aufgabe 2 – Entity Framework
- Entity Framework 6 (Code First)
- `BookingDbContext` mit:
  - `DbSet<PhysicalTicket>`
  - `DbSet<TicketBooking>`
- Verwendung von `int`, `string`, `double`, `bool`, `DateTime`
- Relation Ticket ↔ Buchung

---

## Aufgabe 3 – Repository
- `BookingRepository`
- CRUD-Operationen:
  - Create
  - Read
  - Delete
  - Search
  - Copy
- Validierung gegen Doppelbuchungen

---

## Aufgabe 4 – ViewModel
- Verwendung von `ObservableCollection`
- Properties für alle UI-Bindings
- Commands:
  - Neu
  - Buchen
  - Löschen
  - Kopieren
  - Beenden

---

## Aufgabe 5 – ItemsControl & Details
- Ticketliste als `ListBox`
- Auswahl zeigt Details & Status
- Statuszeile zeigt tagesabhängige Informationen

---

## Aufgabe 6 – Neues Objekt anlegen
- Neues Ticket wird über Menü oder Toolbar angelegt
- Speicherung über Repository
- Sofortige Aktualisierung der GUI

---

## Aufgabe 7 – Suchen
- Suche erfolgt über:
  - ausgewähltes Datum
  - Status der Tickets (frei / belegt)
- Diese Lösung ist fachlich sinnvoll, da Tickets tageweise vergeben werden

---

## Aufgabe 8 – Löschen
- Löschen einer Buchung über Button
- Ticket wird automatisch wieder freigegeben

---

## Aufgabe 9 – Kopieren
- Bestehende Buchung kann kopiert werden
- Neues Datum / Ticket auswählbar
- Keine Mehrfachbuchungen möglich

---

## Aufgabe 10 – Zusätzliche Controls
Verwendete Controls:
- Calendar
- CheckBox
- ToolTip

---

## Aufgabe 11 – Styles
- Zentrale Styles für:
  - Buttons
  - TextBoxen
  - ListBoxItems
- Einheitliches Erscheinungsbild

---

## Fazit
Alle Anforderungen des Prüfungsprojekts wurden vollständig umgesetzt.
Zusätzlich wurde besonderer Wert auf Benutzerfreundlichkeit, klare
Fachlogik und saubere Architektur gelegt.
