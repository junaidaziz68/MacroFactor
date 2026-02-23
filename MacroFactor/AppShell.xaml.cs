using Microsoft.Maui.Controls;

namespace MacroFactor;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Optional: Disable flyout
        FlyoutBehavior = FlyoutBehavior.Disabled;

        // Tab bar colors
        this.Resources["TabBarBackgroundColor"] = Colors.Black;    // Background
        this.Resources["TabBarForegroundColor"] = Colors.White;    // Selected icon/text
        this.Resources["TabBarUnselectedColor"] = Colors.Red;      // Unselected icon/text
    }
}