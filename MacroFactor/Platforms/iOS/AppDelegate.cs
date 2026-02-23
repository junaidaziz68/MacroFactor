using Foundation;
using UIKit;
using Microsoft.Maui;

namespace MacroFactor;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
        {
            var appearance = new UITabBarAppearance();
            appearance.ConfigureWithOpaqueBackground();
            appearance.BackgroundColor = UIColor.Black;

            // Set unselected icon color
            appearance.StackedLayoutAppearance.Normal.IconColor = UIColor.Red;
            appearance.StackedLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.Red
            };

            // Set selected icon color
            appearance.StackedLayoutAppearance.Selected.IconColor = UIColor.White;
            appearance.StackedLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = UIColor.White
            };

            UITabBar.Appearance.StandardAppearance = appearance;

            if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
            {
                UITabBar.Appearance.ScrollEdgeAppearance = appearance;
            }
        }

        return base.FinishedLaunching(app, options);
    }
}