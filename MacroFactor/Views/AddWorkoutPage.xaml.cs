using MacroFactor.ViewModels;
using Microsoft.Maui.Controls;

namespace MacroFactor.Views;

public partial class AddWorkoutPage : ContentPage
{
    public AddWorkoutPage(AddWorkoutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}