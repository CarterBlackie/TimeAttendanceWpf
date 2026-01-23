using TimeAttendanceWpf.UI.Services;

namespace TimeAttendanceWpf.UI.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;

    public MainViewModel(NavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += () => OnPropertyChanged(nameof(CurrentViewModel));
    }

    public ViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;
}
