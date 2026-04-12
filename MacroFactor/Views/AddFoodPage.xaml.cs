using MacroFactor.ViewModels;

namespace MacroFactor.Views;

public partial class AddFoodPage : ContentPage
{
    public AddFoodPage(AddFoodViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
