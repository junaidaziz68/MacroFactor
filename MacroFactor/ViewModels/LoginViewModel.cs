using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MacroFactor.Services;

namespace MacroFactor.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly SupabaseService _supabaseService;

    public LoginViewModel(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;

        LoginCommand = new Command(async () => await Login());
        RegisterCommand = new Command(async () => await Register());
    }

    // =======================
    // Bindable Properties
    // =======================

    private string? email;
    public string? Email
    {
        get => email;
        set
        {
            email = value;
            OnPropertyChanged();
        }
    }

    private string? password;
    public string? Password
    {
        get => password;
        set
        {
            password = value;
            OnPropertyChanged();
        }
    }

    private string? message;
    public string? Message
    {
        get => message;
        set
        {
            message = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }

    // =======================
    // REGISTER
    // =======================

    private async Task Register()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Please enter an email.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
            {
                Message = "Password must be at least 6 characters.";
                return;
            }

            var cleanEmail = Email.Trim();
            var cleanPassword = Password.Trim();

            var client = _supabaseService.GetClient();
            await _supabaseService.InitializeAsync();

            var result = await client.Auth.SignUp(cleanEmail, cleanPassword);

            if (result?.User != null)
                Message = "Registration successful! Check your email.";
            else
                Message = "Registration failed.";
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("429"))
                Message = "Too many attempts. Wait a few minutes.";
            else
                Message = $"Error: {ex.Message}";
        }
    }

    // Login Logic will go herer 

    private async Task Login()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Please enter an email.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                Message = "Please enter a password.";
                return;
            }

            var cleanEmail = Email.Trim();
            var cleanPassword = Password.Trim();

            var client = _supabaseService.GetClient();
            await _supabaseService.InitializeAsync();

            var result = await client.Auth.SignInWithPassword(cleanEmail, cleanPassword);

            if (result?.User != null)
            {
                var profileResponse = await client.From<MacroFactor.Models.UserProfile>()
                                                  .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, result.User.Id)
                                                  .Get();
                
                var profile = profileResponse?.Models?.FirstOrDefault();
                bool hasCompletedOnboarding = profile != null && profile.TargetCalories > 0;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (hasCompletedOnboarding)
                    {
                        Application.Current.MainPage = new AppShell();
                    }
                    else
                    {
                        var onboardingViewModel = new OnboardingViewModel(_supabaseService);
                        Application.Current.MainPage = new NavigationPage(new Views.OnboardingPage(onboardingViewModel));
                    }
                });
            }
              
        }
        catch (Exception ex)
        {
            Message = $"Error: {ex.Message}";
        }
    }

    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}