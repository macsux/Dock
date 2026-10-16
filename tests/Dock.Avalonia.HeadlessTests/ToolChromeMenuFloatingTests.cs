using System;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class ToolChromeMenuFloatingTests
{
    [AvaloniaFact]
    public void Menu_Offers_Float_And_Dock_Only_Outside_A_Floating_Window()
    {
        var app = Application.Current ?? throw new InvalidOperationException("Application is not initialized.");
        var previousStyles = app.Styles.ToList();
        app.Styles.Clear();
        app.Styles.Add(new FluentTheme());
        app.Styles.Add(new DockFluentTheme());
        try
        {
            Assert.Equal(["_Float", "_Dock"], VisibleItems(floating: false).Intersect(["_Float", "_Dock"]));
            // Already floating: Float would open the same tool in another window, and Dock only applies to a pinned tool.
            Assert.Empty(VisibleItems(floating: true).Intersect(["_Float", "_Dock"]));
        }
        finally
        {
            app.Styles.Clear();
            foreach (IStyle style in previousStyles) app.Styles.Add(style);
        }
    }

    private static string[] VisibleItems(bool floating)
    {
        var factory = new Factory();
        var tool = new Tool { Title = "Tool", CanFloat = true, CanPin = true };
        var toolDock = new ToolDock
        {
            VisibleDockables = new AvaloniaList<IDockable> { tool },
            ActiveDockable = tool,
            CanCloseLastDockable = true,
            Factory = factory
        };
        tool.Owner = toolDock;
        Window window;
        if (floating)
        {
            var hostWindow = new HostWindow();
            hostWindow.SetLayout(new RootDock { VisibleDockables = new AvaloniaList<IDockable> { toolDock }, ActiveDockable = toolDock, Factory = factory });
            window = hostWindow;
        }
        else window = new Window { Width = 400, Height = 300, Content = new ToolChromeControl { DataContext = toolDock } };
        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        var chrome = window.GetVisualDescendants().OfType<ToolChromeControl>().Single();
        Assert.Equal(floating, chrome.IsFloating);

        var flyout = Assert.IsType<MenuFlyout>(chrome.ToolFlyout);
        flyout.ShowAt(chrome);
        Dispatcher.UIThread.RunJobs();
        var visible = flyout.Items.OfType<MenuItem>().Where(i => i.IsVisible).Select(i => i.Header as string ?? "").ToArray();
        flyout.Hide();
        window.Close();
        return visible;
    }
}
