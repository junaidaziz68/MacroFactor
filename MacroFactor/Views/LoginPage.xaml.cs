using MacroFactor.ViewModels;

namespace MacroFactor.Views;

public partial class LoginPage : ContentPage
{
    // Constructor receives the ViewModel (MVVM pattern)
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel; // Bind XAML to ViewModel
    }
}