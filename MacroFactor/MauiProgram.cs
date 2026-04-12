using MacroFactor.Services;
using MacroFactor.ViewModels;
using MacroFactor.Views;
using Microsoft.Extensions.Logging;

namespace MacroFactor;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Views and ViewModels
        builder.Services.AddSingleton<SupabaseService>();
        
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<DashboardPage>();
        
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        
        builder.Services.AddTransient<OnboardingViewModel>();
        builder.Services.AddTransient<OnboardingPage>();
        
        builder.Services.AddSingleton<OpenFoodFactsApiService>();
        builder.Services.AddSingleton<AddFoodViewModel>();
        builder.Services.AddTransient<AddFoodPage>();

        builder.Services.AddTransient<AddWorkoutViewModel>();
        builder.Services.AddTransient<AddWorkoutPage>();

        builder.Services.AddTransient<WeightProgressViewModel>();
        builder.Services.AddTransient<WeightProgressPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}