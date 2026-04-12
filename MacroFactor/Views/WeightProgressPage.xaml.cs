using MacroFactor.ViewModels;
namespace MacroFactor.Views;

public partial class WeightProgressPage : ContentPage
{
    public WeightProgressPage(WeightProgressViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.WeightLogs.CollectionChanged += (s, e) => weightGraph.Invalidate();
    }

    private void OnLogClicked(object sender, EventArgs e)
    {
        // Force the hardware GraphicsView to repaint the path whenever manual data modifications occur
        weightGraph.Invalidate();
    }
}
