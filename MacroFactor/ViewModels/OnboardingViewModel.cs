using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MacroFactor.Models;
using MacroFactor.Services;
using MacroFactor.Views;

namespace MacroFactor.ViewModels;

public partial class OnboardingViewModel : ObservableObject
{
    private readonly SupabaseService _supabaseService;

    [ObservableProperty]
    private int age;

    [ObservableProperty]
    private int weight; // in kg

    [ObservableProperty]
    private int height; // in cm

    [ObservableProperty]
    private string gender = "Male"; // Male, Female

    [ObservableProperty]
    private string activityLevel = "Moderately Active"; // Sedentary, Lightly Active, Moderately Active, Very Active, Extra Active

    [ObservableProperty]
    private string goal = "Maintain Weight"; // Lose Weight, Maintain Weight, Gain Weight

    [ObservableProperty]
    private int targetCalories;

    [ObservableProperty]
    private int targetProtein;

    [ObservableProperty]
    private int targetCarbs;

    [ObservableProperty]
    private int targetFats;
    
    [ObservableProperty]
    private bool isCalculated;

    public OnboardingViewModel(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
        Age = 25;
        Weight = 70;
        Height = 175;
    }

    [RelayCommand]
    public void Calculate()
    {
        // Mifflin-St Jeor Equation
        double bmr = (10 * Weight) + (6.25 * Height) - (5 * Age);
        bmr += Gender == "Male" ? 5 : -161;

        double activityMultiplier = ActivityLevel switch
        {
            "Sedentary" => 1.2,
            "Lightly Active" => 1.375,
            "Moderately Active" => 1.55,
            "Very Active" => 1.725,
            "Extra Active" => 1.9,
            _ => 1.55
        };

        double tdee = bmr * activityMultiplier;

        double goalMultiplier = Goal switch
        {
            "Lose Weight" => 0.8,
            "Gain Weight" => 1.2,
            _ => 1.0
        };

        TargetCalories = (int)(tdee * goalMultiplier);

        // Standard macro split: Protein 30%, Fats 30%, Carbs 40%
        TargetProtein = (int)((TargetCalories * 0.30) / 4);
        TargetFats = (int)((TargetCalories * 0.30) / 9);
        TargetCarbs = (int)((TargetCalories * 0.40) / 4);

        IsCalculated = true;
    }

    [RelayCommand]
    public async Task Continue()
    {
        if (!IsCalculated) return;

        var client = _supabaseService.GetClient();
        if (client.Auth.CurrentUser == null) return;

        var profile = new UserProfile
        {
            Id = client.Auth.CurrentUser.Id,
            TargetCalories = TargetCalories,
            TargetProtein = TargetProtein,
            TargetCarbs = TargetCarbs,
            TargetFats = TargetFats
        };

        try
        {
            await client.From<UserProfile>().Update(profile);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage = new AppShell();
            });
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Database Error", $"Could not save your goals to the 'profiles' table. Ensure you have added the correct columns (target_calories, target_protein, target_carbs, target_fats). Details: {ex.Message}", "OK");
            });
        }
    }
}
