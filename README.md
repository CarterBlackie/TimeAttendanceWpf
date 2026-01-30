# TimeAttendanceWpf

## Project Description
TimeAttendanceWpf is a desktop time and attendance application built using C# and WPF (.NET).  
The project models a simplified workforce time-tracking system similar to those used in HR and payroll platforms.

The focus of this project is business logic, architecture, and correctness rather than visual polish.

---

## Current Features
- User selection (Employee or Manager)
- Employee dashboard
  - Clock In
  - Clock Out
  - Validation rules:
    - No double clock-in
    - No clock-out without clock-in
- Manager dashboard placeholder
- MVVM architecture
- View navigation without code-behind logic
- Display real punch history per employee
- Enable and disable Clock In / Clock Out based on current state
- Weekly timesheets
- Overtime calculation (40+ hours per week)
- Time rounding rules (e.g., nearest 15 minutes)
- Manager approval workflow for timesheets
- Persistent storage using SQLite
- Unit tests covering time-tracking edge cases
---

## Architecture
The solution is organized into clear layers:

### Domain
Pure business models with no UI dependencies.
- Employee
- TimePunch
- WorkShift

### Application
Business logic and rules.
- TimeClockService
- Repository abstractions
- In-memory repository implementation

### UI (WPF)
Presentation layer using MVVM.
- Views (XAML)
- ViewModels
- Navigation and session state management

This separation keeps the core logic testable and independent of the UI.

---

## Technology Stack
- C#
- .NET 8
- WPF
- MVVM pattern
- xUnit (test project scaffolded)
- Git and GitHub

---

## Planned Features
The following features are planned for future iterations:

- Enhanced manager dashboard
  - View all submitted timesheets by employee
  - Approve or reject timesheets with required notes
- Pay period views
  - Bi-weekly and monthly pay period summaries
  - Aggregated regular and overtime hours
- Advanced overtime rules
  - Daily overtime thresholds
  - Double-time rules
  - Weekend and holiday premiums
- Break and lunch tracking
  - Paid and unpaid breaks
  - Automatic lunch deduction rules
- Missed punch workflow
  - Employee punch edit requests
  - Manager review and approval
- Holiday and leave support
  - Holiday calendars
  - Paid time off entries
- Timesheet export
  - CSV export per week or pay period
- Configuration settings
  - Overtime thresholds
  - Rounding rules
  - Week start day
- Audit and history tracking
  - Timesheet status change history
  - Approval and rejection metadata
- Additional test coverage
  - Cross-midnight shift edge cases
  - Multi-week overtime scenarios
- UI enhancements
  - Calendar-based week picker
  - Status badges and visual indicators
  - Improved accessibility and keyboard navigation

The current architecture was designed to support these additions without major refactoring.

---

## Purpose
This project was created to demonstrate:
- Practical C# and WPF development
- Realistic business rule implementation
- Clean separation of concerns
- Incremental feature development

It is intended to be easy to explain, extend, and reason about in a technical interview setting.

---

## How to Run
dotnet run --project TimeAttendanceWpf.UI
