using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MacroFactor.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private int totalCalories;

    public int TotalCalories
    {
        get => totalCalories;
        set
        {
            totalCalories = value;
            OnPropertyChanged();
        }
    }

    public DashboardViewModel()
    {
        // TEMP: fake data
        TotalCalories = 1450;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}