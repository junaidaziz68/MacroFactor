using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace MacroFactor.Models;

public partial class Meal : ObservableObject
{
    public string Name { get; set; }

    public ObservableCollection<LoggedFood> Foods { get; } = new();

    public int TotalCalories => Foods.Sum(f => f.Calories);
    public double TotalProtein => Math.Round(Foods.Sum(f => f.Protein), 1);
    public double TotalCarbs => Math.Round(Foods.Sum(f => f.Carbs), 1);
    public double TotalFats => Math.Round(Foods.Sum(f => f.Fats), 1);

    public Meal(string name)
    {
        Name = name;
        Foods.CollectionChanged += (s, e) => 
        {
            OnPropertyChanged(nameof(TotalCalories));
            OnPropertyChanged(nameof(TotalProtein));
            OnPropertyChanged(nameof(TotalCarbs));
            OnPropertyChanged(nameof(TotalFats));
        };
    }
}
