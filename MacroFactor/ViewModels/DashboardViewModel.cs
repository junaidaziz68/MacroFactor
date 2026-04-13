using System.ComponentModel;
using System.Runtime.CompilerServices;
using MacroFactor.Services;

namespace MacroFactor.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private int totalCalories;
    private int _targetCalories;
    private int _targetProtein;
    private int _targetCarbs;
    private int _targetFats;
    private readonly SupabaseService _supabaseService;

    public AddFoodViewModel Tracker { get; }
    
    // These properties do the math: they take your Goal and subtract whatever you've eaten so far today.
    public int RemainingCalories => TargetCalories - Tracker.TotalDailyCalories;
    public double RemainingProtein => TargetProtein - Tracker.TotalDailyProtein;
    public double RemainingCarbs => TargetCarbs - Tracker.TotalDailyCarbs;
    public double RemainingFats => TargetFats - Tracker.TotalDailyFats;

    public Command NavigateToAddFoodCommand { get; }
    public Command NavigateToAddWorkoutCommand { get; }
    public Command LogoutCommand { get; }

    private string _greetingMessage = "Welcome back!";
    public string GreetingMessage
    {
        get => _greetingMessage;
        set { _greetingMessage = value; OnPropertyChanged(); }
    }

    public int TotalCalories
    {
        get => totalCalories;
        set { totalCalories = value; OnPropertyChanged(); }
    }

    public int TargetCalories
    {
        get => _targetCalories;
        set { _targetCalories = value; OnPropertyChanged(); OnPropertyChanged(nameof(RemainingCalories)); }
    }

    public int TargetProtein
    {
        get => _targetProtein;
        set { _targetProtein = value; OnPropertyChanged(); OnPropertyChanged(nameof(RemainingProtein)); }
    }

    public int TargetCarbs
    {
        get => _targetCarbs;
        set { _targetCarbs = value; OnPropertyChanged(); OnPropertyChanged(nameof(RemainingCarbs)); }
    }

    public int TargetFats
    {
        get => _targetFats;
        set { _targetFats = value; OnPropertyChanged(); OnPropertyChanged(nameof(RemainingFats)); }
    }

    public DashboardViewModel(SupabaseService supabaseService, AddFoodViewModel tracker)
    {
        _supabaseService = supabaseService;
        Tracker = tracker;

        // Setup the buttons so that clicking them moves you to the correct tabs
        NavigateToAddFoodCommand = new Command(async () => await Shell.Current.GoToAsync("//AddFoodPage"));
        NavigateToAddWorkoutCommand = new Command(async () => await Shell.Current.GoToAsync("//AddWorkoutPage"));
        
        // Sever the core Supabase tracking token and kill the Shell App navigation root to return them safely back to the Login splash
        LogoutCommand = new Command(async () => {
            await _supabaseService.GetClient().Auth.SignOut();
            MainThread.BeginInvokeOnMainThread(() => 
            {
                var loginVm = new LoginViewModel(_supabaseService);
                Application.Current.MainPage = new NavigationPage(new Views.LoginPage(loginVm));
            });
        });

        _ = LoadGoals();

        // Every time you add a new food, this tells the Dashboard to update the numbers on the screen!
        Tracker.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AddFoodViewModel.TotalDailyCalories) || 
                e.PropertyName == nameof(AddFoodViewModel.TotalDailyProtein)  ||
                e.PropertyName == nameof(AddFoodViewModel.TotalDailyCarbs)  ||
                e.PropertyName == nameof(AddFoodViewModel.TotalDailyFats))
            {
                OnPropertyChanged(nameof(RemainingCalories));
                OnPropertyChanged(nameof(RemainingProtein));
                OnPropertyChanged(nameof(RemainingCarbs));
                OnPropertyChanged(nameof(RemainingFats));
            }
        };
    }

    private async Task LoadGoals()
    {
        try
        {
            var client = _supabaseService.GetClient();
            if (client.Auth.CurrentUser != null)
            {
                // This checks your email address, takes the first part before the '@', and uses it as your name.
                var email = client.Auth.CurrentUser.Email ?? "User";
                var name = email.Split('@')[0];
                if (name.Length > 0)
                    name = char.ToUpper(name[0]) + name.Substring(1); // Capitalize your name!
                    
                GreetingMessage = $"Welcome back, {name}!";

                // Connects to the database to grab your specific diet goals
                var profileResponse = await client.From<MacroFactor.Models.UserProfile>()
                                        .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, client.Auth.CurrentUser.Id)
                                        .Get();
                var profile = profileResponse?.Models?.FirstOrDefault();
                
                if (profile != null && profile.TargetCalories > 0)
                {
                    TotalCalories = profile.TargetCalories;
                    TargetCalories = profile.TargetCalories;
                    TargetProtein = profile.TargetProtein;
                    TargetCarbs = profile.TargetCarbs;
                    TargetFats = profile.TargetFats;
                }
                else
                {
                    // Fallbacks for new users if the database is still initializing!
                    TotalCalories = 2100;
                    TargetCalories = 2100;
                    TargetProtein = 140;
                    TargetCarbs = 220;
                    TargetFats = 60;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading goals: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}