using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MacroFactor.Models;
using MacroFactor.Controls;

namespace MacroFactor.ViewModels;

public class WeightProgressViewModel : INotifyPropertyChanged
{
    private string _currentWeightInput;
    public string CurrentWeightInput
    {
        get => _currentWeightInput;
        set { _currentWeightInput = value; OnPropertyChanged(); }
    }

    public ObservableCollection<WeightLog> WeightLogs { get; } = new();

    public WeightChartDrawable ChartDrawable { get; }

    public ICommand LogWeightCommand { get; }

    public WeightProgressViewModel()
    {
        // Setup the chart painter tool
        ChartDrawable = new WeightChartDrawable();
        
        // Setup what happens when you click the 'Log' button
        LogWeightCommand = new Command(ExecuteLogWeight);

        // We fill in some fake data here from the last 3 days so the graph has a cool slope right away!
        WeightLogs.Add(new WeightLog { Date = DateTime.Now.AddDays(-3), Weight = 175.2 });
        WeightLogs.Add(new WeightLog { Date = DateTime.Now.AddDays(-2), Weight = 174.5 });
        WeightLogs.Add(new WeightLog { Date = DateTime.Now.AddDays(-1), Weight = 173.8 });
        
        UpdateChart();
    }

    private void ExecuteLogWeight()
    {
        // Try to read the text you typed as a number. If it succeeds, save it!
        if (double.TryParse(CurrentWeightInput, out double parsedWeight))
        {
            WeightLogs.Add(new WeightLog
            {
                Date = DateTime.Now,  // Uses exactly right now as the time
                Weight = parsedWeight
            });
            
            // Clear out the text box
            CurrentWeightInput = string.Empty;
            UpdateChart();
        }
    }

    private void UpdateChart()
    {
        ChartDrawable.Data = WeightLogs.ToList();
        OnPropertyChanged(nameof(ChartDrawable));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
