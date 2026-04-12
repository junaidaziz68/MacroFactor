using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MacroFactor.Models;
using MacroFactor.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Controls;

namespace MacroFactor.ViewModels;

public partial class AddFoodViewModel : ObservableObject
{
    private readonly OpenFoodFactsApiService _offService;

    [ObservableProperty]
    private string searchQuery;

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private bool isDiaryViewVisible = true;

    [ObservableProperty]
    private bool isSearchViewVisible = false;

    [ObservableProperty]
    private bool isEditFoodVisible = false;

    [ObservableProperty]
    private string editFoodName;

    [ObservableProperty]
    private string editFoodCalories;

    [ObservableProperty]
    private string editFoodProtein;

    [ObservableProperty]
    private string editFoodCarbs;

    [ObservableProperty]
    private string editFoodFats;

    // The meal the user is currently searching food for
    private Meal _currentTargetMeal;
    private LoggedFood _editingLoggedFood;

    public ObservableCollection<Meal> Meals { get; } = new();
    public ObservableCollection<FoodSearchResult> SearchResults { get; } = new();

    public int TotalDailyCalories => Meals.Sum(m => m.TotalCalories);
    public double TotalDailyProtein => Meals.Sum(m => m.TotalProtein);
    public double TotalDailyCarbs => Meals.Sum(m => m.TotalCarbs);
    public double TotalDailyFats => Meals.Sum(m => m.TotalFats);

    public AddFoodViewModel(OpenFoodFactsApiService offService)
    {
        _offService = offService;

        // Initialize 5 meals by default
        Meals.Add(new Meal("Meal 1"));
        Meals.Add(new Meal("Meal 2"));
        Meals.Add(new Meal("Meal 3"));
        Meals.Add(new Meal("Meal 4"));
        Meals.Add(new Meal("Meal 5"));

        // Subscribe to changes in Meals so Daily Totals recalculate
        foreach (var meal in Meals)
        {
            meal.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(TotalDailyCalories));
                OnPropertyChanged(nameof(TotalDailyProtein));
                OnPropertyChanged(nameof(TotalDailyCarbs));
                OnPropertyChanged(nameof(TotalDailyFats));
            };
        }
    }

    [RelayCommand]
    private void OpenSearchForMeal(Meal targetMeal)
    {
        _currentTargetMeal = targetMeal;
        SearchQuery = string.Empty;
        SearchResults.Clear();
        
        IsDiaryViewVisible = false;
        IsSearchViewVisible = true;
    }

    [RelayCommand]
    private void CancelSearch()
    {
        IsSearchViewVisible = false;
        IsDiaryViewVisible = true;
        _currentTargetMeal = null;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditFoodVisible = false;
        IsDiaryViewVisible = true;
        _editingLoggedFood = null;
    }

    [RelayCommand]
    private async Task SearchFoodAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        IsSearching = true;
        SearchResults.Clear();

        try
        {
            var jsonResponse = await _offService.SearchFoodAsync(SearchQuery);
            
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("products", out var productsElement))
            {
                foreach (var item in productsElement.EnumerateArray())
                {
                    var id = item.TryGetProperty("code", out var idElem) ? idElem.GetString() : Guid.NewGuid().ToString();
                    var name = "Unknown Product";
                    if (item.TryGetProperty("product_name_en", out var nameEn) && !string.IsNullOrWhiteSpace(nameEn.GetString()))
                        name = nameEn.GetString();
                    else if (item.TryGetProperty("product_name", out var nameStd) && !string.IsNullOrWhiteSpace(nameStd.GetString()))
                        name = nameStd.GetString();
                        
                    if (name == "Unknown Product") continue;

                    double cals = 0, prot = 0, carbs = 0, fats = 0;
                    if (item.TryGetProperty("nutriments", out var nutriments))
                    {
                        if (nutriments.TryGetProperty("energy-kcal_100g", out var cElem) && cElem.ValueKind == JsonValueKind.Number)
                            cals = cElem.GetDouble();
                        
                        if (nutriments.TryGetProperty("proteins_100g", out var pElem) && pElem.ValueKind == JsonValueKind.Number)
                            prot = pElem.GetDouble();
                            
                        if (nutriments.TryGetProperty("carbohydrates_100g", out var carbElem) && carbElem.ValueKind == JsonValueKind.Number)
                            carbs = carbElem.GetDouble();
                            
                        if (nutriments.TryGetProperty("fat_100g", out var fElem) && fElem.ValueKind == JsonValueKind.Number)
                            fats = fElem.GetDouble();
                    }

                    // Format specifically so that the existing UI item parser still works beautifully
                    var description = $"Per 100g - Calories: {(int)cals}kcal | Fat: {fats}g | Carbs: {carbs}g | Protein: {prot}g";
                    
                    SearchResults.Add(new FoodSearchResult 
                    { 
                        FoodId = id, 
                        Name = name, 
                        Description = description 
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search failed: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Search Error", ex.Message, "OK");
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private void SelectFood(FoodSearchResult result)
    {
        if (result == null || _currentTargetMeal == null) return;

        // Parse macros
        var food = new LoggedFood
        {
            Id = result.FoodId,
            Name = result.Name
        };

        try
        {
            var desc = result.Description;
            if (desc.Contains("Calories: "))
            {
                var calPart = desc.Substring(desc.IndexOf("Calories: ") + 10);
                food.Calories = int.Parse(calPart.Substring(0, calPart.IndexOf("kcal")).Trim());
            }
            if (desc.Contains("Protein: "))
            {
                var protPart = desc.Substring(desc.IndexOf("Protein: ") + 9);
                food.Protein = double.Parse(protPart.Substring(0, protPart.IndexOf("g")).Trim());
            }
            if (desc.Contains("Carbs: "))
            {
                var carbPart = desc.Substring(desc.IndexOf("Carbs: ") + 7);
                food.Carbs = double.Parse(carbPart.Substring(0, carbPart.IndexOf("g")).Trim());
            }
            if (desc.Contains("Fat: "))
            {
                var fatPart = desc.Substring(desc.IndexOf("Fat: ") + 5);
                food.Fats = double.Parse(fatPart.Substring(0, fatPart.IndexOf("g")).Trim());
            }
        }
        catch { /* Ignore parsing errors for now */ }

        // Execute UI updates cleanly on the main thread to avoid layout lag
        MainThread.BeginInvokeOnMainThread(() => 
        {
            _currentTargetMeal.Foods.Add(food);
            IsSearchViewVisible = false;
            IsDiaryViewVisible = true;
            _currentTargetMeal = null;
        });
    }

    [RelayCommand]
    private void DeleteFood(LoggedFood food)
    {
        if (food == null) return;
        var meal = Meals.FirstOrDefault(m => m.Foods.Contains(food));
        if (meal != null)
        {
            meal.Foods.Remove(food);
        }
    }

    [RelayCommand]
    private void EditFood(LoggedFood food)
    {
        if (food == null) return;
        _editingLoggedFood = food;

        EditFoodName = food.Name;
        EditFoodCalories = food.Calories.ToString();
        EditFoodProtein = food.Protein.ToString();
        EditFoodCarbs = food.Carbs.ToString();
        EditFoodFats = food.Fats.ToString();

        IsDiaryViewVisible = false;
        IsSearchViewVisible = false;
        IsEditFoodVisible = true;
    }

    [RelayCommand]
    private void SaveEditedFood()
    {
        if (_editingLoggedFood == null) return;

        _editingLoggedFood.Name = EditFoodName;
        _editingLoggedFood.Calories = int.TryParse(EditFoodCalories, out var c) ? c : 0;
        _editingLoggedFood.Protein = double.TryParse(EditFoodProtein, out var p) ? p : 0;
        _editingLoggedFood.Carbs = double.TryParse(EditFoodCarbs, out var cb) ? cb : 0;
        _editingLoggedFood.Fats = double.TryParse(EditFoodFats, out var f) ? f : 0;

        var meal = Meals.FirstOrDefault(m => m.Foods.Contains(_editingLoggedFood));
        if (meal != null)
        {
            var index = meal.Foods.IndexOf(_editingLoggedFood);
            meal.Foods[index] = _editingLoggedFood; // Triggers UI update
        }

        IsEditFoodVisible = false;
        IsDiaryViewVisible = true;
        _editingLoggedFood = null;
    }
}

public class FoodSearchResult
{
    public string FoodId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
