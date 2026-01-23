namespace TimeAttendanceWpf.UI.ViewModels;

public sealed class SelectEmployeeViewModel : ViewModelBase
{
    public List<string> Employees { get; } = new()
    {
        "Dawson (Employee)",
        "Alex (Manager)"
    };

    private string? _selectedEmployee;
    public string? SelectedEmployee
    {
        get => _selectedEmployee;
        set
        {
            _selectedEmployee = value;
            OnPropertyChanged();
        }
    }

    public SelectEmployeeViewModel()
    {
        SelectedEmployee = Employees[0];
    }
}
